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
            .Include(r => r.Players)
            .Where(r => r.AccountId == accountId)
            .Where(r => !tournamentId.HasValue || r.TournamentId == tournamentId.Value)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<CustomRanking?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.CustomRankings
            .AsNoTracking()
            .Include(r => r.Tournament)
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
