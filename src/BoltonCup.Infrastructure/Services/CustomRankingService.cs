using BoltonCup.Core;
using BoltonCup.Core.Commands;
using BoltonCup.Core.Exceptions;
using BoltonCup.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BoltonCup.Infrastructure.Services;

public class CustomRankingService(
    BoltonCupDbContext _dbContext
) : ICustomRankingService
{
    public async Task<IReadOnlyList<CustomRanking>> GetForAccountAsync(int accountId, int? tournamentId = null,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.CustomRankings
            .AsNoTracking()
            .Include(r => r.Tournament)
            .Include(r => r.Account)
            .Include(r => r.Players)
            .Where(r => r.AccountId == accountId)
            .Where(r => !tournamentId.HasValue || r.TournamentId == tournamentId.Value)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CustomRanking>> GetSharedWithAccountAsync(int accountId, int? tournamentId = null,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.CustomRankings
            .AsNoTracking()
            .Include(r => r.Tournament)
            .Include(r => r.Account)
            .Include(r => r.Players)
            .Where(r => r.SharedWith.Any(s => s.SharedWithAccountId == accountId))
            .Where(r => !tournamentId.HasValue || r.TournamentId == tournamentId.Value)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<CustomRanking?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.CustomRankings
            .AsNoTracking()
            .Include(r => r.Tournament)
            .Include(r => r.Account)
            .Include(r => r.SharedWith)
            .Include(r => r.Players.OrderBy(p => p.Rank))
                .ThenInclude(p => p.Player)
                .ThenInclude(p => p.Account)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<int> CreateAsync(CreateCustomRankingCommand command, CancellationToken cancellationToken = default)
    {
        if (await _dbContext.Tournaments.AllAsync(t => t.Id != command.TournamentId, cancellationToken))
            throw new EntityNotFoundException(nameof(Tournament), command.TournamentId);

        var ranking = new CustomRanking
        {
            AccountId = command.OwnerAccountId,
            TournamentId = command.TournamentId,
            Title = command.Title,
            Players = await BuildSeededPlayersAsync(command.TournamentId, cancellationToken),
        };

        _dbContext.CustomRankings.Add(ranking);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ranking.Id;
    }

    public async Task<int> CloneAsync(int sourceRankingId, int ownerAccountId, string? title = null, CancellationToken cancellationToken = default)
    {
        var source = await _dbContext.CustomRankings
                         .AsNoTracking()
                         .Include(r => r.Players)
                         .FirstOrDefaultAsync(r => r.Id == sourceRankingId, cancellationToken)
                     ?? throw new EntityNotFoundException(nameof(CustomRanking), sourceRankingId);

        var clone = new CustomRanking
        {
            AccountId = ownerAccountId,
            TournamentId = source.TournamentId,
            Title = string.IsNullOrWhiteSpace(title) ? $"Copy of {source.Title}" : title.Trim(),
            Players = source.Players
                .Select(p => new CustomRankingPlayer
                {
                    PlayerId = p.PlayerId,
                    Rank = p.Rank,
                    GamesPlayed = p.GamesPlayed,
                    TotalPoints = p.TotalPoints,
                })
                .ToList(),
        };

        _dbContext.CustomRankings.Add(clone);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return clone.Id;
    }

    public async Task UpdateAsync(int id, UpdateCustomRankingCommand command, CancellationToken cancellationToken = default)
    {
        var ranking = await _dbContext.CustomRankings
            .Include(r => r.Players)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(CustomRanking), id);

        if (!string.IsNullOrWhiteSpace(command.Title))
        {
            ranking.Title = command.Title;
        }

        if (command.OrderedPlayerIds is { } orderedPlayerIds)
        {
            // OrderedPlayerIds is the caller's ordering of (a subset of) the ranking's players. Listed
            // players take ranks in the given order; any players not listed — e.g. a registrant added by
            // reconciliation after the editor loaded — keep their place after them, then all are renumbered.
            var existingByPlayerId = ranking.Players.ToDictionary(p => p.PlayerId);
            if (orderedPlayerIds.Count != orderedPlayerIds.Distinct().Count())
            {
                throw new InvalidOperationException("Ordered players must not contain duplicates.");
            }
            if (!orderedPlayerIds.All(existingByPlayerId.ContainsKey))
            {
                throw new InvalidOperationException("Ordered players must all belong to the ranking.");
            }

            var listed = new HashSet<int>(orderedPlayerIds);
            var ordered = orderedPlayerIds
                .Select(x => existingByPlayerId[x])
                .Concat(ranking.Players
                    .Where(p => !listed.Contains(p.PlayerId))
                    .OrderBy(p => p.Rank))
                .ToList();

            for (var i = 0; i < ordered.Count; i++)
            {
                ordered[i].Rank = i + 1;
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        return _dbContext.CustomRankings
            .Where(r => r.Id == id)
            .ExecuteDeleteAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CustomRankingShareInfo>> GetSharesAsync(int rankingId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.CustomRankingShares
            .AsNoTracking()
            .Where(s => s.CustomRankingId == rankingId)
            .OrderBy(s => s.SharedWithAccount.FirstName)
            .ThenBy(s => s.SharedWithAccount.LastName)
            .Select(s => new CustomRankingShareInfo(
                s.SharedWithAccountId,
                s.SharedWithAccount.FirstName + " " + s.SharedWithAccount.LastName,
                s.SharedWithAccount.Email,
                s.SharedWithAccount.Avatar))
            .ToListAsync(cancellationToken);
    }

    public async Task AddShareAsync(int rankingId, int accountId, CancellationToken cancellationToken = default)
    {
        var ranking = await _dbContext.CustomRankings
                          .FirstOrDefaultAsync(r => r.Id == rankingId, cancellationToken)
                      ?? throw new EntityNotFoundException(nameof(CustomRanking), rankingId);

        if (accountId == ranking.AccountId)
            throw new InvalidOperationException("Cannot share a ranking with its owner.");

        var isGm = await _dbContext.Teams
            .AnyAsync(t => t.TournamentId == ranking.TournamentId && t.GeneralManagers.Any(g => g.Id == accountId), cancellationToken);
        if (!isGm)
        {
            throw new InvalidOperationException("Rankings can only be shared with a GM of the tournament.");
        }

        var exists = await _dbContext.CustomRankingShares
            .AnyAsync(s => s.CustomRankingId == rankingId && s.SharedWithAccountId == accountId, cancellationToken);
        if (exists)
            return;

        _dbContext.CustomRankingShares.Add(new CustomRankingShare
        {
            CustomRankingId = rankingId,
            SharedWithAccountId = accountId,
        });
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task RemoveShareAsync(int rankingId, int accountId, CancellationToken cancellationToken = default)
    {
        return _dbContext.CustomRankingShares
            .Where(s => s.CustomRankingId == rankingId && s.SharedWithAccountId == accountId)
            .ExecuteDeleteAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<RankingInviteCandidate>> SearchInvitableGmsAsync(int rankingId, string? query,
        int limit = 5, CancellationToken cancellationToken = default)
    {
        var ranking = await _dbContext.CustomRankings
                          .AsNoTracking()
                          .FirstOrDefaultAsync(r => r.Id == rankingId, cancellationToken)
                      ?? throw new EntityNotFoundException(nameof(CustomRanking), rankingId);

        query = query?.Trim();
        var hasQuery = !string.IsNullOrEmpty(query);
        var pattern = $"%{query}%";

        var gmAccountIds = _dbContext.Teams
            .Where(t => t.TournamentId == ranking.TournamentId)
            .SelectMany(t => t.GeneralManagers.Select(g => g.Id));

        var alreadyShared = _dbContext.CustomRankingShares
            .Where(s => s.CustomRankingId == rankingId)
            .Select(s => s.SharedWithAccountId);

        var accounts = _dbContext.Accounts
            .AsNoTracking()
            .Where(a => gmAccountIds.Contains(a.Id))
            .Where(a => a.Id != ranking.AccountId)
            .Where(a => !alreadyShared.Contains(a.Id));

        if (hasQuery)
        {
            accounts = accounts.Where(a =>
                EF.Functions.ILike(a.FirstName + " " + a.LastName, pattern)
                || EF.Functions.ILike(a.Email, pattern));
        }

        return await accounts
            .OrderBy(a => a.FirstName)
            .ThenBy(a => a.LastName)
            .Take(limit)
            .Select(a => new RankingInviteCandidate(a.Id, a.FirstName + " " + a.LastName, a.Email))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlySet<int>> ReconcileAsync(int rankingId, CancellationToken cancellationToken = default)
    {
        var ranking = await _dbContext.CustomRankings
                          .Include(r => r.Players)
                          .FirstOrDefaultAsync(r => r.Id == rankingId, cancellationToken)
                      ?? throw new EntityNotFoundException(nameof(CustomRanking), rankingId);

        var poolPlayers = await LoadPoolWithStatsAsync(ranking.TournamentId, cancellationToken);
        var poolIds = poolPlayers.Select(p => p.Id).ToHashSet();
        var existingIds = ranking.Players.Select(p => p.PlayerId).ToHashSet();

        // Newly-registered players are auto-ranked into their default position: each is inserted ahead of
        // the first existing entry with a lower points-per-game (mirroring how the ranking is first seeded).
        var newPlayers = poolPlayers.Where(p => !existingIds.Contains(p.Id)).ToList();
        if (newPlayers.Count > 0)
        {
            var ordered = ranking.Players.OrderBy(p => p.Rank).ToList();

            foreach (var player in newPlayers.OrderByDescending(ComputePointsPerGame))
            {
                var (gamesPlayed, totalPoints) = ComputeStats(player);
                var entry = new CustomRankingPlayer
                {
                    CustomRankingId = ranking.Id,
                    PlayerId = player.Id,
                    GamesPlayed = gamesPlayed,
                    TotalPoints = totalPoints,
                };

                var index = ordered.FindIndex(e => e.PointsPerGame < entry.PointsPerGame);
                if (index < 0)
                {
                    ordered.Add(entry);
                }
                else
                {
                    ordered.Insert(index, entry);
                }

                ranking.Players.Add(entry);
            }

            for (var i = 0; i < ordered.Count; i++)
            {
                ordered[i].Rank = i + 1;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        // Stale = ranking entries whose player is no longer in the tournament pool. Dormant today
        // (Player rows are never deleted, and the FK cascades), but surfaced so the GM can remove them.
        return existingIds.Where(id => !poolIds.Contains(id)).ToHashSet();
    }

    public async Task RemovePlayerAsync(int rankingId, int playerId, CancellationToken cancellationToken = default)
    {
        var player = await _dbContext.CustomRankingPlayers
            .FirstOrDefaultAsync(p => p.CustomRankingId == rankingId && p.PlayerId == playerId, cancellationToken);
        if (player is null)
        {
            return;
        }

        _dbContext.CustomRankingPlayers.Remove(player);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<List<CustomRankingPlayer>> BuildSeededPlayersAsync(int tournamentId, CancellationToken cancellationToken)
    {
        var players = await LoadPoolWithStatsAsync(tournamentId, cancellationToken);

        return players
            .Select(player =>
            {
                var (gamesPlayed, totalPoints) = ComputeStats(player);
                return new CustomRankingPlayer
                {
                    PlayerId = player.Id,
                    GamesPlayed = gamesPlayed,
                    TotalPoints = totalPoints,
                };
            })
            .OrderByDescending(p => p.PointsPerGame)
            .Select((player, index) =>
            {
                player.Rank = index + 1;
                return player;
            })
            .ToList();
    }

    private Task<List<Player>> LoadPoolWithStatsAsync(int tournamentId, CancellationToken cancellationToken)
    {
        return _dbContext.Players
            .Include(p => p.Account)
            .ThenInclude(account => account.Players)
            .ThenInclude(player => player.SkaterGameLogs)
            .Include(player => player.Account)
            .ThenInclude(account => account.Players)
            .ThenInclude(player => player.GoalieGameLogs)
            .Where(p => p.TournamentId == tournamentId)
            .ToListAsync(cancellationToken);
    }

    private static (int GamesPlayed, int TotalPoints) ComputeStats(Player player)
    {
        var skaterLogs = player.Account.Players
            .SelectMany(p => p.SkaterGameLogs)
            .ToList();
        var goalieLogs = player.Account.Players
            .SelectMany(p => p.GoalieGameLogs)
            .ToList();

        return (skaterLogs.Count + goalieLogs.Count, skaterLogs.Sum(x => x.Points));
    }

    private static double ComputePointsPerGame(Player player)
    {
        var (gamesPlayed, totalPoints) = ComputeStats(player);
        return gamesPlayed == 0 ? 0 : (double)totalPoints / gamesPlayed;
    }
}
