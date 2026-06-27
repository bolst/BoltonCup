using BoltonCup.Infrastructure.Data;
using BoltonCup.Core;
using BoltonCup.Core.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace BoltonCup.Infrastructure.Services;

public class TournamentPlayerInfoService(BoltonCupDbContext _dbContext) : ITournamentPlayerInfoService
{
    public async Task<TournamentPlayerInfoContext> GetAsync(int tournamentId, int accountId,
        CancellationToken cancellationToken = default)
    {
        var info = await _dbContext.TournamentPlayerInfos
            .AsNoTracking()
            .Include(e => e.GameAvailabilities)
            .FirstOrDefaultAsync(e => e.AccountId == accountId && e.TournamentId == tournamentId, cancellationToken);

        var player = await _dbContext.Players
            .AsNoTracking()
            .Include(p => p.Team)
            .FirstOrDefaultAsync(p => p.AccountId == accountId && p.TournamentId == tournamentId, cancellationToken)
            ?? throw new AccountNotInTournamentException(accountId, tournamentId);

        var gamesQuery = _dbContext.Games
            .AsNoTracking()
            .Include(g => g.Tournament)
            .Include(g => g.HomeTeam)
            .Include(g => g.AwayTeam)
            .Where(g => g.TournamentId == tournamentId);

        // Filter to the player's games only once they have a team; undrafted players see every game.
        if (player.TeamId is { } teamId)
        {
            gamesQuery = gamesQuery.Where(g =>
                g.HomeTeamId == teamId || g.AwayTeamId == teamId || (g.HomeTeamId == null && g.AwayTeamId == null));
        }

        var games = await gamesQuery
            .OrderBy(g => g.GameTime)
            .ToListAsync(cancellationToken);

        // If the requester is the GM of a team in this tournament, surface that team's song selections so the
        // same form can offer GM-only goal/win song controls.
        var managedTeam = await _dbContext.Teams
            .AsNoTracking()
            .Include(t => t.GoalSongTrack)
            .Include(t => t.WinSongTrack)
            .Where(t => t.TournamentId == tournamentId && t.GmAccountId == accountId)
            .Select(t => new ManagedTeamSongs(t.Id, t.Name, t.GoalSongTrack, t.WinSongTrack))
            .FirstOrDefaultAsync(cancellationToken);

        return new TournamentPlayerInfoContext(info, games, player.Team, managedTeam);
    }

    public async Task UpsertAsync(UpsertTournamentPlayerInfoCommand command,
        CancellationToken cancellationToken = default)
    {
        var tournament = await _dbContext.Tournaments.FindAsync([command.TournamentId], cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Tournament), command.TournamentId);

        if (!tournament.IsPlayerInfoOpen)
            throw new InvalidOperationException("Player info is not open for this tournament.");

        var player = await _dbContext.Players
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.AccountId == command.AccountId && p.TournamentId == command.TournamentId, cancellationToken)
            ?? throw new AccountNotInTournamentException(command.AccountId, command.TournamentId);

        var validGameIdsQuery = _dbContext.Games
            .AsNoTracking()
            .Where(g => g.TournamentId == command.TournamentId);

        // Filter to the player's games only once they have a team; undrafted players can set every game.
        if (player.TeamId is { } teamId)
        {
            validGameIdsQuery = validGameIdsQuery.Where(g =>
                g.HomeTeamId == teamId || g.AwayTeamId == teamId || g.HomeTeamId == null || g.AwayTeamId == null);
        }

        var validGameIds = await validGameIdsQuery
            .Select(g => g.Id)
            .ToListAsync(cancellationToken);

        var availabilities = command.GameAvailability
            .Where(a => validGameIds.Contains(a.GameId))
            .GroupBy(a => a.GameId)
            .ToDictionary(g => g.Key, g => g.Last().Availability);

        var entry = await _dbContext.TournamentPlayerInfos
            .Include(e => e.GameAvailabilities)
            .SingleOrDefaultAsync(e => e.AccountId == command.AccountId && e.TournamentId == command.TournamentId,
                cancellationToken);

        if (entry is null)
        {
            entry = new TournamentPlayerInfo
            {
                TournamentId = command.TournamentId,
                AccountId = command.AccountId,
            };
            ApplyChanges(entry, command, availabilities);
            await _dbContext.TournamentPlayerInfos.AddAsync(entry, cancellationToken);
        }
        else
        {
            ApplyChanges(entry, command, availabilities);
        }

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException) when (_dbContext.Entry(entry).State == EntityState.Added)
        {
            // Lost an insert race against a concurrent first-time upsert for the same
            // (account, tournament); the unique index rejected our INSERT. Reload the
            // row the winner created and apply our changes as an update instead.
            _dbContext.Entry(entry).State = EntityState.Detached;

            var existing = await _dbContext.TournamentPlayerInfos
                .Include(e => e.GameAvailabilities)
                .SingleAsync(e => e.AccountId == command.AccountId && e.TournamentId == command.TournamentId,
                    cancellationToken);
            ApplyChanges(existing, command, availabilities);

            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private static void ApplyChanges(TournamentPlayerInfo entry, UpsertTournamentPlayerInfoCommand command,
        IReadOnlyDictionary<int, GameAvailability> availabilities)
    {
        entry.SongTrackId = command.Song?.TrackId;
        entry.SongName = command.Song?.Name;
        entry.SongArtist = command.Song?.Artist;
        entry.SongAlbumArtUrl = command.Song?.AlbumArtUrl;

        var existing = entry.GameAvailabilities.ToDictionary(a => a.GameId);

        foreach (var availability in entry.GameAvailabilities.Where(a => !availabilities.ContainsKey(a.GameId)).ToList())
        {
            entry.GameAvailabilities.Remove(availability);
        }

        foreach (var (gameId, state) in availabilities)
        {
            if (existing.TryGetValue(gameId, out var row))
            {
                row.Availability = state;
            }
            else
            {
                entry.GameAvailabilities.Add(new TournamentPlayerGameAvailability
                {
                    GameId = gameId,
                    Availability = state,
                });
            }
        }
    }
}
