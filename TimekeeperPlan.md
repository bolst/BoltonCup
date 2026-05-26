# Timekeeper Application — High-Level Plan

## Context

Timekeepers at Bolton Cup games need an app to record game events (goals, penalties, game state changes, game stars) in real-time. They'll be on-location with unreliable internet, so the app must work offline and sync when connectivity is available. The app will use the existing WebAPI to fetch game/player data and push recorded events.

## Decision: Blazor WASM PWA

Build a new **Blazor WebAssembly Progressive Web App** (`BoltonCup.Timekeeper`), following the same pattern as Draft/WebClient/Auth.

**Why:**
- Timekeepers use MacBooks — PWA on desktop browsers is rock-solid
- Matches the existing project pattern (zero new tooling)
- Reuses `BoltonCup.Common` (MudBlazor, SDK, auth) and `BoltonCup.WebAPI.Client` (NSwag SDK)
- Installable via "Add to Home Screen" in Chrome/Edge — no app store
- Offline support via service worker (caches app shell) + IndexedDB (caches data)

**Auth:** Cookie-based login through the existing Auth flow. Add a `Timekeeper` role to gate write endpoints.

## Scope (MVP)

- Record goals (scorer, assists, period, time)
- Record penalties (player, infraction, duration, period, time)
- Game state transitions (Pending → InProgress → Completed, period tracking)
- Game stars (1st/2nd/3rd star selection)
- Offline-first with background sync
- **Out of scope:** Live spectator broadcast via SignalR (future phase)

## Architecture

### 1. New Blazor WASM Project

```
src/BoltonCup.Timekeeper/
├── BoltonCup.Timekeeper.csproj     (BlazorWebAssembly, references Common + WebAPI.Client)
├── Program.cs                       (same bootstrap pattern as Draft app)
├── wwwroot/
│   ├── index.html                   (PWA manifest + service worker registration)
│   ├── manifest.json
│   ├── service-worker.js
│   └── appsettings.json
├── Components/
│   ├── App.razor
│   ├── Layout/
│   │   └── MainLayout.razor
│   └── Pages/
│       ├── Home.razor               (select a game from today's schedule)
│       └── GameConsole.razor         (main timekeeping screen)
├── Dialogs/
│   ├── GoalDialog.razor             (record a goal — pick scorer, assists)
│   ├── PenaltyDialog.razor          (record a penalty — pick player, infraction)
│   └── GameStarsDialog.razor        (select 1st/2nd/3rd stars)
└── Services/
    ├── TimekeeperStateService.cs    (current game state, event list, period tracking)
    ├── OfflineStore.cs              (IndexedDB wrapper via JS interop)
    └── SyncService.cs              (background sync — pushes pending events to API)
```

**Template to follow:** `src/BoltonCup.Draft/` — same `Program.cs` bootstrap, same project references.

### 2. New WebAPI Endpoints

The API currently only has GET endpoints for games. The Timekeeper needs write endpoints.

Add to `GamesController` (or a new `TimekeepingController`):

| Method | Route | Purpose |
|--------|-------|---------|
| `PATCH` | `/api/games/{id}/state` | Update game state (Pending → InProgress → Completed) |
| `POST` | `/api/games/{id}/goals` | Record a goal |
| `PUT` | `/api/games/{id}/goals/{goalId}` | Edit a goal |
| `DELETE` | `/api/games/{id}/goals/{goalId}` | Delete a goal |
| `POST` | `/api/games/{id}/penalties` | Record a penalty |
| `PUT` | `/api/games/{id}/penalties/{penaltyId}` | Edit a penalty |
| `DELETE` | `/api/games/{id}/penalties/{penaltyId}` | Delete a penalty |
| `POST` | `/api/games/{id}/stars` | Set game stars |

All gated by `[Authorize(Roles = "Timekeeper")]` (or `"Admin,Timekeeper"`).

**Supporting backend work:**
- **Core:** Add command classes (`CreateGoalCommand`, `CreatePenaltyCommand`, `UpdateGameStateCommand`) and an `IGameService` or extend `IGameRepository` with write methods
- **Infrastructure:** Implement the write operations in the repository/service layer
- **Mapping:** Add request DTOs and mapper methods for the new endpoints
- **NSwag:** Regenerate `BoltonCup.WebAPI.Client` so the Timekeeper gets typed SDK methods

**Idempotency:** Accept an optional `CorrelationId` on POST requests to prevent duplicate records from sync retries.

### 3. Offline-First Architecture

**Pre-game (online):** When a timekeeper selects a game, fetch and cache to IndexedDB:
- Game details (teams, schedule, venue)
- Home + away team rosters (player id, name, jersey number, position)

**During game (offline-capable):** Every event (goal, penalty, state change) is:
1. Written immediately to an IndexedDB event queue
2. UI reads from local state — never blocks on network
3. `SyncService` attempts to POST each pending event on a timer (~5-10s)
4. On success → mark as synced. On failure → retry next cycle.

**Sync status UI:** A visible indicator showing online/offline state, pending event count, and last sync time.

**Event queue record shape:**
```
{
  CorrelationId: guid,
  GameId: int,
  EventType: "Goal" | "Penalty" | "GameStateChange" | "GameStar",
  Payload: { ... },
  CreatedAt: datetime,
  SyncStatus: "Pending" | "Synced",
  LastAttempt: datetime?
}
```

### 4. Authorization

- Add a `Timekeeper` role to the Identity system (alongside existing `Admin`)
- Assign the role to timekeeper accounts
- Write endpoints require `Timekeeper` or `Admin` role
- Timekeepers authenticate via the existing cookie-based Auth flow (same as Draft app)

## Key Files to Modify/Reference

| File | Role |
|------|------|
| `src/BoltonCup.Draft/Program.cs` | Template for new app bootstrap |
| `src/BoltonCup.Draft/Services/DraftStateService.cs` | Pattern for real-time state + connection management |
| `src/BoltonCup.WebAPI/Controllers/GamesController.cs` | Extend with write endpoints |
| `src/BoltonCup.Core/Entities/Goal.cs` | Existing entity — write operations target this |
| `src/BoltonCup.Core/Entities/Penalty.cs` | Existing entity — write operations target this |
| `src/BoltonCup.Core/Entities/Game.cs` | GameState transitions |
| `src/BoltonCup.Core/Entities/GameStar.cs` | Star selections |
| `src/BoltonCup.Common/ServiceCollectionExtensions.cs` | Shared service registration |
| `src/BoltonCup.SessionStorage/` | Pattern for browser storage JS interop |
| `src/BoltonCup.WebAPI/Auth/` | Add Timekeeper role/policy |

## Implementation Phases

1. **API endpoints** — Add write endpoints for goals, penalties, game state, stars. Regenerate NSwag client.
2. **App shell** — Scaffold the Blazor WASM PWA project. Verify it can fetch and display games (online only).
3. **Game console UI** — Build the timekeeping screen with MudBlazor. Wire up to API (online only first).
4. **Offline support** — Add IndexedDB storage, event queue, sync service, PWA manifest + service worker.

## Risks

- **IndexedDB in Blazor WASM** requires JS interop — manageable, the SessionStorage project shows the pattern.
- **Service worker cache invalidation** — need a visible "update available" prompt so timekeepers get app updates.
- **No existing write endpoints** — the largest backend work is building CRUD for goals/penalties/state in the API.

## Verification

- Run the Timekeeper app locally, log in, select a game, record goals/penalties
- Disconnect network, record events, verify they queue in IndexedDB
- Reconnect, verify events sync to the API and appear in the Admin/WebClient
- Test on Chrome/Edge on macOS with "Install as app" flow
