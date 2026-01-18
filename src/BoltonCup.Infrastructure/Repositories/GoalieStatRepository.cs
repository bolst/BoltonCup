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
            .Include(p => p.Player)
            .Include(p => p.Tournament)
            .Include(p => p.Team)
            .ConditionalWhere(p => p.TournamentId == query.TournamentId, query.TournamentId.HasValue)
            .ConditionalWhere(p => p.TeamId == query.TeamId, query.TeamId.HasValue)
            .OrderByDescending(p => p.SavePercentage)
            .ThenBy(p => p.GoalsAgainstAverage)
            .ThenByDescending(p => p.Shutouts)
            .ThenByDescending(p => p.Wins)
            .ThenByDescending(p => p.Saves)
            .ToPaginatedListAsync(query);
    }       
    
    public async Task<CollectionResult<T>> GetAllAsync<T>(GetGoalieStatsQuery query)
        where T : IMappable<GoalieStat, T>
    {
        return await _context.GoalieStats
            .ConditionalWhere(p => p.TournamentId == query.TournamentId, query.TournamentId.HasValue)
            .ConditionalWhere(p => p.TeamId == query.TeamId, query.TeamId.HasValue)
            .OrderByDescending(p => p.SavePercentage)
            .ThenBy(p => p.GoalsAgainstAverage)
            .ThenByDescending(p => p.Shutouts)
            .ThenByDescending(p => p.Wins)
            .ThenByDescending(p => p.Saves)
            .ProjectTo<GoalieStat, T>()
            .ToPaginatedListAsync(query);
    }       
    
    public async Task<GoalieStat?> GetByIdAsync(int id)
    {
        return await _context.GoalieStats
            .Include(p => p.Player)
            .Include(p => p.Team)
            .Include(p => p.Tournament)
            .FirstOrDefaultAsync(p => p.PlayerId == id);
    }

    public async Task<T?> GetByIdAsync<T>(int id)
        where T : IMappable<GoalieStat, T>
    {
        return await _context.GoalieStats
            .Where(p => p.PlayerId == id)
            .ProjectTo<GoalieStat, T>()
            .FirstOrDefaultAsync();
    }
}