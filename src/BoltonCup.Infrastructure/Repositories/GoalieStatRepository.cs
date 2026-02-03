using BoltonCup.Infrastructure.Data;
using BoltonCup.Core;
using BoltonCup.Infrastructure.Extensions;
using BoltonCup.Core.Mappings;
using Microsoft.EntityFrameworkCore;

namespace BoltonCup.Infrastructure.Repositories;



public class GoalieStatRepository(BoltonCupDbContext _context) : IGoalieStatRepository
{
    public async Task<CollectionResult<GoalieStat>> GetAllAsync(GetGoalieStatsQuery query)
    {
        return await _context.GoalieStats
            .AsNoTracking()
            .ConditionalWhere(p => p.TournamentId == query.TournamentId, query.TournamentId.HasValue)
            .ConditionalWhere(p => p.TeamId == query.TeamId, query.TeamId.HasValue)
            .OrderBy(x => x.AccountId)
            .ThenByDescending(x => x.GameTime)
            .GroupBy(x => x.AccountId)
            .Select(g => new GoalieStat
            {
                PlayerId = g.First().PlayerId,
                GoalsAgainst = g.Sum(x => x.GoalsAgainst),
                ShotsAgainst = g.Sum(x => x.ShotsAgainst),
                Saves = g.Sum(x => x.Saves),
                Shutouts = g.Sum(x => x.Shutouts),
                Wins = g.Sum(x => x.Wins),
                SavePercentage = g.Sum(x => x.Saves) / (double)g.Sum(x => x.ShotsAgainst),
                GoalsAgainstAverage = g.Average(x => x.GoalsAgainst),
                GamesPlayed = g.Sum(x => x.GamesPlayed),
                Goals = g.Sum(x => x.Goals),
                Assists = g.Sum(x => x.Assists),
                Points = g.Sum(x => x.Points),
                PenaltyMinutes = g.Sum(x => x.PenaltyMinutes),
                AccountId = g.Key,
                FirstName = g.First().FirstName,
                LastName = g.First().LastName,
                Position = g.First().Position,
                JerseyNumber = g.First().JerseyNumber,
                Birthday = g.First().Birthday,
                ProfilePicture = g.First().ProfilePicture
            })
            .ApplySorting(query, x => x
                .OrderByDescending(p => p.Shutouts)
                .ThenByDescending(p => p.Saves)
                .ThenByDescending(p => p.Wins)
                .ThenBy(p => p.GamesPlayed)
            )
            .ToPaginatedListAsync(query);
    }       
    
    public async Task<CollectionResult<T>> GetAllAsync<T>(GetGoalieStatsQuery query)
        where T : IMappable<GoalieStat, T>
    {
        return await _context.GoalieStats
            .AsNoTracking()
            .ConditionalWhere(p => p.TournamentId == query.TournamentId, query.TournamentId.HasValue)
            .ConditionalWhere(p => p.TeamId == query.TeamId, query.TeamId.HasValue)
            .OrderBy(x => x.AccountId)
            .ThenByDescending(x => x.GameTime)
            .GroupBy(x => x.AccountId)
            .Select(g => new GoalieStat
            {
                PlayerId = g.First().PlayerId,
                GoalsAgainst = g.Sum(x => x.GoalsAgainst),
                ShotsAgainst = g.Sum(x => x.ShotsAgainst),
                Saves = g.Sum(x => x.Saves),
                Shutouts = g.Sum(x => x.Shutouts),
                Wins = g.Sum(x => x.Wins),
                SavePercentage = g.Sum(x => x.ShotsAgainst) == 0 ? 0 : g.Sum(x => x.Saves) / (double)g.Sum(x => x.ShotsAgainst),
                GoalsAgainstAverage = g.Average(x => x.GoalsAgainst),
                GamesPlayed = g.Sum(x => x.GamesPlayed),
                Goals = g.Sum(x => x.Goals),
                Assists = g.Sum(x => x.Assists),
                Points = g.Sum(x => x.Points),
                PenaltyMinutes = g.Sum(x => x.PenaltyMinutes),
                AccountId = g.Key,
                FirstName = g.First().FirstName,
                LastName = g.First().LastName,
                Position = g.First().Position,
                JerseyNumber = g.First().JerseyNumber,
                Birthday = g.First().Birthday,
                ProfilePicture = g.First().ProfilePicture,
                TeamId = g.First().TeamId,
                TeamName = g.First().TeamName,
                TeamNameShort = g.First().TeamNameShort,
                TeamAbbreviation = g.First().TeamAbbreviation,
                TeamLogoUrl = g.First().TeamLogoUrl,
            })
            .ApplySorting(query, x => x
                .OrderByDescending(p => p.Shutouts)
                .ThenByDescending(p => p.Saves)
                .ThenByDescending(p => p.Wins)
                .ThenBy(p => p.GamesPlayed)
            )
            .ProjectTo<GoalieStat, T>()
            .ToPaginatedListAsync(query);
    }       
    
    public async Task<GoalieStat?> GetByIdAsync(int id)
    {
        return await _context.GoalieStats
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.PlayerId == id);
    }

    public async Task<T?> GetByIdAsync<T>(int id)
        where T : IMappable<GoalieStat, T>
    {
        return await _context.GoalieStats
            .AsNoTracking()
            .ProjectToFirstOrDefaultAsync<GoalieStat, T>(p => p.PlayerId == id);
    }
}