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
            var existingPlayerIds = ranking.Players.Select(p => p.PlayerId).ToHashSet();
            if (orderedPlayerIds.Count != existingPlayerIds.Count ||
                !orderedPlayerIds.ToHashSet().SetEquals(existingPlayerIds))
            {
                throw new InvalidOperationException("Ordered players must match the ranking's existing player set exactly.");
            }

            var rankByPlayerId = orderedPlayerIds
                .Select((playerId, index) => (playerId, rank: index + 1))
                .ToDictionary(x => x.playerId, x => x.rank);

            foreach (var player in ranking.Players)
            {
                player.Rank = rankByPlayerId[player.PlayerId];
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
            .AnyAsync(t => t.TournamentId == ranking.TournamentId && t.GmAccountId == accountId, cancellationToken);
        if (!isGm)
            throw new InvalidOperationException("Rankings can only be shared with a GM of the tournament.");

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
            .Where(t => t.TournamentId == ranking.TournamentId && t.GmAccountId != null)
            .Select(t => t.GmAccountId!.Value);

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

    private async Task<List<CustomRankingPlayer>> BuildSeededPlayersAsync(int tournamentId, CancellationToken cancellationToken)
    {
        var players = await _dbContext.Players
            .Include(p => p.Account)
            .ThenInclude(account => account.Players)
            .ThenInclude(player => player.SkaterGameLogs)
            .Include(player => player.Account)
            .ThenInclude(account => account.Players)
            .ThenInclude(player => player.GoalieGameLogs)
            .Where(p => p.TournamentId == tournamentId)
            .ToListAsync(cancellationToken);

        return players
            .Select(player =>
            {
                var skaterLogs = player.Account.Players
                    .SelectMany(p => p.SkaterGameLogs)
                    .ToList();
                var goalieLogs = player.Account.Players
                    .SelectMany(p => p.GoalieGameLogs)
                    .ToList();

                return new CustomRankingPlayer
                {
                    PlayerId = player.Id,
                    GamesPlayed = skaterLogs.Count + goalieLogs.Count,
                    TotalPoints = skaterLogs.Sum(x => x.Points),
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
}
