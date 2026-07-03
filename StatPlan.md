# Replace stat materialized views with in-memory computed + cached stats

## Context

Player/goalie stats are currently served from two Postgres **materialized views**
(`core.mv_skater_game_logs`, `core.mv_goalie_game_logs`) mapped into EF as `SkaterStat` /
`GoalieStat` (`.ToView(...)`, one row per game/player). The views are pure derivations of the
event tables (`goals`, `penalties`, `games`, `players`, …) but **nothing in the app ever runs
`REFRESH MATERIALIZED VIEW`** — so stats are effectively frozen at migration time and never
reflect live games. The matview approach also fights EF (raw-SQL objects EF can't manage, manual
column mapping, dead nav collections).

**Goal:** delete the matviews and compute stats in C# from the raw event tables, **for completed
games only**, cached in `IMemoryCache` and invalidated when a game is marked `Completed` (plus a
TTL backstop for post-completion scoresheet corrections). Data volume is one-tournament-scale, so
recompute is trivially cheap. The repository interfaces stay identical, so **controllers, mapper,
DTOs, queries, and the generated API client are untouched.**

## Approach

**No new service** (per steer) — the compute + cache + invalidation logic lives directly in the two
repositories. Mirror the pure-static-`Compute` style of
`src/BoltonCup.Infrastructure/Services/StandingsService.cs` (completed-games-only, DB-free math) but
keep it as `public static` methods on the repos so they're unit-testable.

### 1. Repositories own load + compute + cache
`SkaterStatRepository.cs` / `GoalieStatRepository.cs`: inject `BoltonCupDbContext` **and**
`IMemoryCache` (already registered via `AddMemoryCache()` in `ServiceCollectionExtensions.cs`).

- Each repo caches its per-game rows under one fixed key (`"stats:skater-game-logs"` /
  `"stats:goalie-game-logs"`) with an absolute-expiry TTL backstop (~10 min), via
  `IMemoryCache.GetOrCreateAsync`. Cache holds **all completed games across all tournaments** — needed
  because `GetCareerStatsAsync` and nullable `TournamentId` queries span tournaments; all filtering
  happens downstream in memory.
- On a cache miss, load once (all `AsNoTracking`):
  - `Games` where `GameState == GameState.Completed && HomeTeamId != null && AwayTeamId != null`,
    `.Include(g => g.Goals).Include(g => g.Penalties)`
  - `Players.Include(p => p.Account)`, `Teams`, `Tournaments` (into dictionaries)
  then call the static compute. (Minor duplicate loading across the two repos is acceptable at this
  data scale and only happens on cache miss.)
- `InvalidateCache()` (added to each interface) removes the repo's key.

### 2. Pure static compute (replicating the unified-view SQL, completed-only)
Static methods `ComputeSkaterLogs(...)` / `ComputeGoalieLogs(...)` returning per-game rows
(`GamesPlayed = 1` each), faithfully porting
`src/BoltonCup.Infrastructure/Data/Migrations/20260417134906_UnifiedGameLogMaterializedView.cs`
(lines 26–219). Note the C# property names differ from the SQL columns
(`Goal.GoalPlayerId/Assist1PlayerId/Assist2PlayerId`, `Penalty.DurationMinutes`, `Account.Avatar`,
`Team.Logo`):

- **Roster:** for each completed game (both teams set), each `Player` whose `TeamId` is the home or
  away team → one row. Skater = `Position != "goalie"`; goalie = `Position == "goalie"`.
  Opponent = the other team (mirrors the view `CASE`).
- **Skater row:** `goals` = count of `Goals` with `GoalPlayerId == player` in that game;
  `assists` = count where `Assist1PlayerId == player` + count where `Assist2PlayerId == player`;
  `penalty_minutes` = sum of `Penalty.DurationMinutes` for player/game; `points = goals + assists`.
- **Goalie row:** `goals_against` = opponent team's goal count that game;
  `shutouts = goals_against == 0 ? 1 : 0`; `wins = myTeamGoals > oppGoals ? 1 : 0`;
  `shots_against = saves = 0`, `save_percentage = 0`, `goals_against_average = goals_against`
  (unchanged from the view — no source data); plus the goalie's own goals/assists/PIM as above.
- Denormalize the same account/team/opponent/game/tournament fields the view exposed
  (`ProfilePicture` ← `Account.Avatar`, `TeamLogoUrl`/`OpponentLogoUrl` ← `Team.Logo`,
  `TournamentActive` ← `Tournament.IsActive`, etc.).

### 3. Repositories delegate to the service (signatures unchanged)
`SkaterStatRepository.cs` / `GoalieStatRepository.cs`: inject `IPlayerStatsService` instead of
`BoltonCupDbContext`. Replace `_context.SkaterStats` / `_context.GoalieStats` with
`(await _stats.GetSkaterGameLogsAsync(ct)).AsQueryable()`, then keep the **existing**
`ConditionalWhere → OrderBy → GroupBy(AccountId) → Select(sum) → ApplySorting → page` chain verbatim.
`GetCareerStatsAsync` still resolves account ids from `_context.Players`… — keep a `DbContext` (or a
players lookup) for that one query, or move the account-id resolution into the repo via the service's
loaded players. Simplest: repo keeps `BoltonCupDbContext` **and** takes `IPlayerStatsService`.

`ConditionalWhere` (`QueryableExtensions.cs`) and `ApplySorting` (`SortQueryExtensions.cs`, uses
`System.Linq.Dynamic.Core`) both work over in-memory `IQueryable`. Only `ToPagedListAsync`
(`PagedList.cs`) is EF-only (`CountAsync`/`ToListAsync`) — **add a synchronous overload**:

```csharp
public static PagedList<T> ToPagedList<T>(this IEnumerable<T> source, IPaginationQuery query)
{
    var list = source as IReadOnlyList<T> ?? source.ToList();
    var items = list.Skip((query.Page - 1) * query.Size).Take(query.Size).ToList();
    return new PagedList<T>(items, list.Count, query.Page, query.Size);
}
```

### 4. Cache invalidation hook
`src/BoltonCup.Infrastructure/Services/GameWriteService.cs` — `UpdateStateAsync` is the **only**
place `GameState` is written and both online + offline-sync completion paths funnel through it.
Inject `IPlayerStatsService` and call `_stats.Invalidate()` after `SaveChangesAsync`.
(Completed-only + completion-trigger + TTL is the agreed scope; post-completion scoresheet edits and
trades self-heal within the TTL. Optionally also invalidate from goal/penalty writes later — out of
scope here.)

### 5. Remove the matview mappings
`src/BoltonCup.Infrastructure/Data/BoltonCupDbContext.cs`:
- Delete `DbSet<GoalieStat> GoalieStats` (line 24) and `DbSet<SkaterStat> SkaterStats` (line 28).
- Delete the `modelBuilder.Entity<GoalieStat>(...)` (~489–551) and `Entity<SkaterStat>(...)`
  (~750–805) `.ToView(...)` config blocks.

Remove the now-dead view nav collections referenced only by those configs:
- `Game.cs` — `SkaterGameLogs`, `GoalieGameLogs`
- `Team.cs` — `SkaterGameLogs`, `GoalieGameLogs`
- `Player.cs` — `SkaterGameLogs`, `GoalieGameLogs`

Keep `SkaterStat.cs` / `GoalieStat.cs` as plain result POCOs (their `Player/Team/Opponent/Game` navs
were never populated on the read path and can stay or be trimmed).

### 6. Drop the materialized views (EF migration)
`.ToView` objects aren't managed by EF migrations, so add a migration with **manual SQL**:

```
dotnet ef migrations add DropStatMaterializedViews --project src/BoltonCup.Infrastructure \
  --startup-project src/BoltonCup.WebAPI --context BoltonCupDbContext
```

Then edit it:
- `Up`: `DROP MATERIALIZED VIEW IF EXISTS core.mv_skater_game_logs;` and `…mv_goalie_game_logs;`
- `Down`: recreate both (copy the `CREATE MATERIALIZED VIEW …` + unique-index SQL from
  `20260417134906_UnifiedGameLogMaterializedView.cs`) for reversibility.

## Files

- **New:** `BoltonCup.Core/Interfaces/Services/IPlayerStatsService.cs`,
  `BoltonCup.Infrastructure/Services/PlayerStatsService.cs`, the new EF migration.
- **Edit:** `SkaterStatRepository.cs`, `GoalieStatRepository.cs`,
  `Extensions/PagedList.cs` (sync overload), `Services/GameWriteService.cs`,
  `Data/BoltonCupDbContext.cs`, `Entities/{Game,Team,Player}.cs`.
- **Untouched (verify only):** `ISkaterStatRepository`/`IGoalieStatRepository`, all controllers,
  `Mapper`, DTOs, queries/validators, generated client.

## Verification

1. **Build** server side: `dotnet build src/BoltonCup.WebAPI` (regenerates OpenAPI) then
   `dotnet build src/BoltonCup.WebAPI.Client`. WASM projects may fail at emcc natively — verify
   managed compile with `-p:WasmBuildNative=false` if building Timekeeper/WebClient.
2. **Unit-test the pure compute** (`tests/BoltonCup.WebAPI.Tests`, xUnit + FluentAssertions, mirrors
   `StandingsService` tests): feed a small games/goals/penalties/players fixture to
   `ComputeSkaterLogs`/`ComputeGoalieLogs` and assert goals/assists/PIM/points, goalie GA/shutout/win,
   opponent attribution, and that **non-completed games contribute nothing**.
3. **Migration:** `dotnet ef database update` against a dev DB; confirm both matviews are dropped and
   stat endpoints still return data.
4. **End-to-end:** run WebAPI, hit `GET /api/skaterstats`, `/api/goaliestats`, `/api/system`
   (featured leaders), and `GET /api/games/{id}` (career stats). Then mark a game `Completed` via
   `PATCH /api/games/{id}/state` and confirm the next stats read reflects it immediately (cache
   invalidated), where the old matview would have been stale.
