using BoltonCup.Application.Extensions;
using BoltonCup.Core;
using BoltonCup.Core.Commands;
using BoltonCup.Core.Exceptions;
using BoltonCup.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace BoltonCup.Application.Services;

/// <summary>
/// Franchise reads, brand/owner edits, merge and move. Every method uses its own short-lived context from the
/// factory, so it is safe to call from the Blazor Server Admin app as well as the API. Merge and move each
/// commit in a single <c>SaveChangesAsync</c>, which EF Core runs in one transaction.
/// </summary>
sealed class FranchiseService(
    IDbContextFactory<BoltonCupDbContext> _dbContextFactory,
    IStorageService _storageService,
    IAssetKeyGenerator _assetKeyGenerator,
    ITelemetry _telemetry)
    : IFranchiseService
{
    const int LeaderCount = 10;
    const int MinGoalieGamesPlayed = 2;

    public async Task<IReadOnlyList<FranchiseSummary>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var franchises = await db.Franchises.AsNoTracking().ToListAsync(cancellationToken);
        var teamFranchiseIds = await db.Teams
            .AsNoTracking()
            .Select(t => t.FranchiseId)
            .ToListAsync(cancellationToken);
        var championFranchiseIds = await db.Tournaments
            .AsNoTracking()
            .Where(t => t.WinningTeam != null)
            .Select(t => t.WinningTeam!.FranchiseId)
            .ToListAsync(cancellationToken);

        var seasonCounts = teamFranchiseIds.CountBy(id => id).ToDictionary();
        var titleCounts = championFranchiseIds.CountBy(id => id).ToDictionary();

        return franchises
            .Select(f => new FranchiseSummary(f, titleCounts.GetValueOrDefault(f.Id), seasonCounts.GetValueOrDefault(f.Id)))
            .OrderByDescending(s => s.TitleCount)
            .ThenBy(s => s.Franchise.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public async Task<FranchiseDetail?> GetDetailBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var franchise = await db.Franchises
            .AsNoTracking()
            .Include(f => f.Owners)
            .FirstOrDefaultAsync(f => f.Slug == slug, cancellationToken);
        if (franchise is null)
        {
            return null;
        }

        var id = franchise.Id;
        using var operation = _telemetry.StartOperation("franchise.detail", "franchise.id", id);

        var teams = await db.Teams
            .AsNoTracking()
            .Include(t => t.Tournament)
            .Include(t => t.GeneralManagers)
            .Where(t => t.FranchiseId == id)
            .ToListAsync(cancellationToken);
        var teamIds = teams.Select(t => t.Id).ToList();

        var games = await db.Games
            .AsNoTracking()
            .Include(g => g.Goals)
            .Where(g => g.GameState == GameState.Completed)
            .Where(g => (g.HomeTeamId.HasValue && teamIds.Contains(g.HomeTeamId.Value))
                || (g.AwayTeamId.HasValue && teamIds.Contains(g.AwayTeamId.Value)))
            .ToListAsync(cancellationToken);

        var titles = await db.Tournaments
            .AsNoTracking()
            .Where(t => t.WinningTeam != null && t.WinningTeam.FranchiseId == id)
            .OrderBy(t => t.StartDate)
            .ToListAsync(cancellationToken);

        var skaterRows = await db.SkaterStats
            .AsNoTracking()
            .Where(s => teamIds.Contains(s.TeamId))
            .Select(s => new SkaterRow(
                s.AccountId, s.PlayerId, s.FirstName, s.LastName, s.ProfilePicture, s.GameTime, s.TournamentId,
                s.GamesPlayed, s.Goals, s.Assists, s.Points))
            .ToListAsync(cancellationToken);
        var goalieRows = await db.GoalieStats
            .AsNoTracking()
            .Where(s => teamIds.Contains(s.TeamId))
            .Select(s => new GoalieRow(
                s.AccountId, s.PlayerId, s.FirstName, s.LastName, s.ProfilePicture, s.GameTime, s.TournamentId,
                s.GamesPlayed, s.Wins, s.Shutouts, s.Saves, s.ShotsAgainst, s.GoalsAgainst))
            .ToListAsync(cancellationToken);

        var records = FranchiseRecordCalculator.Compute(teams, games);
        // Team.Tournament is typed non-null but is null for a team without a tournament.
        var seasons = teams
            .OrderByDescending(t => ((Tournament?)t.Tournament)?.StartDate)
            .ThenByDescending(t => t.Id)
            .Select(t =>
            {
                var tournament = t.Tournament;
                return new FranchiseSeason(
                    t,
                    tournament,
                    records.ByTeam[t.Id],
                    t.GeneralManagers.ToList(),
                    tournament?.WinningTeamId == t.Id);
            })
            .ToList();

        return new FranchiseDetail
        {
            Franchise = franchise,
            Owners = franchise.Owners.ToList(),
            Titles = titles,
            AllTime = records.AllTime,
            IntraFranchiseGames = records.IntraFranchiseGames,
            Seasons = seasons,
            SkaterLeaders = BuildSkaterLeaders(skaterRows),
            GoalieLeaders = BuildGoalieLeaders(goalieRows),
        };
    }

    // Only the stat columns the leader boards read, so the wide stat views are not fetched in full.
    sealed record SkaterRow(int AccountId, int PlayerId, string FirstName, string LastName, string? ProfilePicture, DateTime GameTime, 
                            int TournamentId, int GamesPlayed, int Goals, int Assists, int Points);

    sealed record GoalieRow(int AccountId, int PlayerId, string FirstName, string LastName, string? ProfilePicture, DateTime GameTime, 
                            int TournamentId, int GamesPlayed, int Wins, int Shutouts, int Saves, int ShotsAgainst, int GoalsAgainst);

    // Name and picture come from each player's most recent row, so a renamed account shows its latest name.
    static IReadOnlyList<FranchiseSkaterLeader> BuildSkaterLeaders(IEnumerable<SkaterRow> rows) => rows
        .GroupBy(s => s.AccountId)
        .Select(g =>
        {
            var latest = g.MaxBy(s => s.GameTime)!;
            return new FranchiseSkaterLeader(
                g.Key,
                latest.PlayerId,
                latest.FirstName,
                latest.LastName,
                latest.ProfilePicture,
                Seasons: g.Select(s => s.TournamentId).Distinct().Count(),
                GamesPlayed: g.Sum(s => s.GamesPlayed),
                Goals: g.Sum(s => s.Goals),
                Assists: g.Sum(s => s.Assists),
                Points: g.Sum(s => s.Points));
        })
        .OrderByDescending(l => l.Points)
        .ThenByDescending(l => l.Goals)
        .ThenBy(l => l.GamesPlayed)
        .ThenBy(l => l.LastName, StringComparer.OrdinalIgnoreCase)
        .Take(LeaderCount)
        .ToList();

    static IReadOnlyList<FranchiseGoalieLeader> BuildGoalieLeaders(IEnumerable<GoalieRow> rows) => rows
        .GroupBy(s => s.AccountId)
        .Select(g =>
        {
            var latest = g.MaxBy(s => s.GameTime)!;
            var gamesPlayed = g.Sum(s => s.GamesPlayed);
            var saves = g.Sum(s => s.Saves);
            var shotsAgainst = g.Sum(s => s.ShotsAgainst);
            var goalsAgainst = g.Sum(s => s.GoalsAgainst);
            return new FranchiseGoalieLeader(
                g.Key,
                latest.PlayerId,
                latest.FirstName,
                latest.LastName,
                latest.ProfilePicture,
                Seasons: g.Select(s => s.TournamentId).Distinct().Count(),
                GamesPlayed: gamesPlayed,
                Wins: g.Sum(s => s.Wins),
                Shutouts: g.Sum(s => s.Shutouts),
                Saves: saves,
                ShotsAgainst: shotsAgainst,
                SavePercentage: shotsAgainst == 0 ? 0 : (double)saves / shotsAgainst,
                GoalsAgainstAverage: gamesPlayed == 0 ? 0 : (double)goalsAgainst / gamesPlayed);
        })
        .Where(l => l.GamesPlayed >= MinGoalieGamesPlayed)
        .OrderByDescending(l => l.Wins)
        .ThenBy(l => l.GoalsAgainstAverage)
        .ThenByDescending(l => l.GamesPlayed)
        .ThenBy(l => l.LastName, StringComparer.OrdinalIgnoreCase)
        .Take(LeaderCount)
        .ToList();

    public async Task<IReadOnlyList<string>> ReserveSlugsAsync(IReadOnlyList<SlugCandidate> candidates, CancellationToken cancellationToken = default)
    {
        if (candidates.Count == 0)
        {
            return [];
        }

        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        return await SlugReservation.ReserveAsync(candidates, SlugsOwnedByOthers(db), cancellationToken);
    }

    static Func<List<int>, IQueryable<string>> SlugsOwnedByOthers(BoltonCupDbContext db) => ids => db.Franchises
        .AsNoTracking()
        .Where(f => !ids.Contains(f.Id))
        .Select(f => f.Slug);

    public async Task<Franchise> CreateAsync(CreateFranchiseCommand command, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var candidate = new SlugCandidate(0, SlugGenerator.Generate(command.Name, Franchise.SlugFallback));
        var reserved = await SlugReservation.ReserveAsync([candidate], SlugsOwnedByOthers(db), cancellationToken);

        var franchise = new Franchise
        {
            Name = command.Name,
            Slug = reserved[0],
            NameShort = command.NameShort,
            Abbreviation = command.Abbreviation,
            PrimaryColorHex = command.PrimaryColorHex,
            SecondaryColorHex = command.SecondaryColorHex,
            TertiaryColorHex = command.TertiaryColorHex,
        };
        db.Franchises.Add(franchise);
        await db.SaveChangesAsync(cancellationToken);
        _telemetry.TrackEvent("franchise.created", "franchise.id", franchise.Id);

        return franchise;
    }

    public async Task UpdateBrandAsync(int id, UpdateFranchiseCommand command, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var franchise = await db.Franchises.FirstOrDefaultAsync(f => f.Id == id, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Franchise), id);

        franchise.Name = command.Name;
        franchise.NameShort = command.NameShort;
        franchise.Abbreviation = command.Abbreviation;
        franchise.PrimaryColorHex = command.PrimaryColorHex;
        franchise.SecondaryColorHex = command.SecondaryColorHex;
        franchise.TertiaryColorHex = command.TertiaryColorHex;

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var franchise = await db.Franchises.FirstOrDefaultAsync(f => f.Id == id, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Franchise), id);
        if (await db.Teams.AnyAsync(t => t.FranchiseId == id, cancellationToken))
        {
            throw new InvalidOperationException($"Franchise {id} still has teams; move or merge them first.");
        }

        db.Franchises.Remove(franchise);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateLogoAsync(int id, string tempKey, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        await _storageService.UpdateAssetAsync<Franchise>(
            db,
            _assetKeyGenerator,
            f => f.Id == id,
            f => f.Logo,
            tempKey,
            id.ToString(),
            cancellationToken);
    }

    public async Task UpdateBannerAsync(int id, string tempKey, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        await _storageService.UpdateAssetAsync<Franchise>(
            db,
            _assetKeyGenerator,
            f => f.Id == id,
            f => f.Banner,
            tempKey,
            id.ToString(),
            cancellationToken);
    }

    public async Task SetOwnersAsync(int id, IReadOnlyCollection<int> accountIds, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        if (!await db.Franchises.AnyAsync(f => f.Id == id, cancellationToken))
        {
            throw new EntityNotFoundException(nameof(Franchise), id);
        }

        var existing = await db.FranchiseOwners
            .Where(o => o.FranchiseId == id)
            .ToListAsync(cancellationToken);

        var desired = accountIds.ToHashSet();
        var currentIds = existing.Select(o => o.AccountId).ToHashSet();

        var removed = existing.Where(o => !desired.Contains(o.AccountId)).ToList();
        var addedIds = desired.Where(accountId => !currentIds.Contains(accountId)).Order().ToArray();

        db.FranchiseOwners.RemoveRange(removed);

        foreach (var accountId in addedIds)
        {
            db.FranchiseOwners.Add(new FranchiseOwner { FranchiseId = id, AccountId = accountId });
        }

        await db.SaveChangesAsync(cancellationToken);
        _telemetry.TrackEvent(
            "franchise.owners_set",
            "franchise.id", id,
            "owner.count", desired.Count,
            "owner.added.ids", addedIds,
            "owner.removed.ids", removed.Select(o => o.AccountId).Order().ToArray());
    }

    public async Task MergeAsync(int sourceId, int targetId, CancellationToken cancellationToken = default)
    {
        if (sourceId == targetId)
        {
            throw new InvalidOperationException("A franchise cannot be merged into itself.");
        }

        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var source = await db.Franchises
            .Include(f => f.Teams)
            .Include(f => f.Owners)
            .FirstOrDefaultAsync(f => f.Id == sourceId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Franchise), sourceId);
        var target = await db.Franchises
            .Include(f => f.Owners)
            .FirstOrDefaultAsync(f => f.Id == targetId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Franchise), targetId);

        var movedTeams = source.Teams.ToList();
        foreach (var team in movedTeams)
        {
            team.Franchise = target;
        }

        var targetOwnerIds = target.Owners.Select(o => o.Id).ToHashSet();
        var addedOwners = source.Owners.Where(o => !targetOwnerIds.Contains(o.Id)).ToList();
        foreach (var owner in addedOwners)
        {
            target.Owners.Add(owner);
        }

        // Deleting the source cascades to its franchise_owners rows.
        db.Franchises.Remove(source);
        await db.SaveChangesAsync(cancellationToken);
        _telemetry.TrackEvent(
            "franchise.merged",
            "franchise.source.id", sourceId,
            "franchise.target.id", targetId,
            "team.count", movedTeams.Count,
            "owner.count", addedOwners.Count);
    }

    public async Task MoveTeamAsync(int teamId, int targetFranchiseId, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var team = await db.Teams.FirstOrDefaultAsync(t => t.Id == teamId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Team), teamId);
        if (!await db.Franchises.AnyAsync(f => f.Id == targetFranchiseId, cancellationToken))
        {
            throw new EntityNotFoundException(nameof(Franchise), targetFranchiseId);
        }

        var sourceFranchiseId = team.FranchiseId;
        team.FranchiseId = targetFranchiseId;
        await db.SaveChangesAsync(cancellationToken);
        _telemetry.TrackEvent(
            "franchise.team_moved",
            "team.id", teamId,
            "franchise.source.id", sourceFranchiseId,
            "franchise.target.id", targetFranchiseId);
    }
}