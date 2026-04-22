using BoltonCup.Infrastructure.Data;
using BoltonCup.Infrastructure.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace BoltonCup.Infrastructure.Services;

public interface IPlayerDraftRankingService
{
    Task RefreshDataAsync(int tournamentId, CancellationToken cancellationToken = default);
}

public class PlayerDraftRankingService(BoltonCupDbContext _dbContext) : IPlayerDraftRankingService
{
    public async Task RefreshDataAsync(int tournamentId, CancellationToken cancellationToken = default)
    {
        var players = await _dbContext.Players
            .Include(p => p.Account)
                .ThenInclude(account => account.Players)
                .ThenInclude(player => player.SkaterGameLogs)
            .Include(player => player.Account)
                .ThenInclude(account => account.Players)
                .ThenInclude(player => player.GoalieGameLogs)
            .Where(p => p.TournamentId == tournamentId)
            .Where(p => _dbContext.PlayerDraftRankings.All(pdr => pdr.PlayerId != p.Id))
            .ToListAsync(cancellationToken: cancellationToken);

        var rankings = players
            .Select(player =>
            {
                var skaterLogs = player.Account.Players
                    .SelectMany(p => p.SkaterGameLogs)
                    .ToList();
                var goalieLogs = player.Account.Players
                    .SelectMany(p => p.GoalieGameLogs)
                    .ToList();

                var totalPoints = skaterLogs.Sum(x => x.Points);
                return new PlayerDraftRanking
                {
                    PlayerId = player.Id,
                    TournamentId = tournamentId,
                    GamesPlayed = skaterLogs.Count + goalieLogs.Count,
                    TotalPoints = totalPoints,
                    IsChampion = false, // TODO
                    DraftRanking = skaterLogs.Count == 0 ? 0 : (double)totalPoints / skaterLogs.Count,
                    OverrideRanking = false,
                };
            });

        _dbContext.PlayerDraftRankings.AddRange(rankings);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}