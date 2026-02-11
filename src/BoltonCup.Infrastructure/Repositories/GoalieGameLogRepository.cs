using BoltonCup.Infrastructure.Data;
using BoltonCup.Core;
using BoltonCup.Infrastructure.Extensions;
using BoltonCup.Core.Mappings;
using BoltonCup.Core.Values;
using Microsoft.EntityFrameworkCore;

namespace BoltonCup.Infrastructure.Repositories;



public class GoalieGameLogRepository(BoltonCupDbContext _context) : IGoalieGameLogRepository
{
    public async Task<CollectionResult<GoalieGameLog>> GetAllAsync(GetGoalieGameLogsQuery query)
    {
        return await _context.GoalieGameLogs
            .AsNoTracking()
            .Include(p => p.Player)
            .Include(p => p.Team)
            .Include(p => p.OpponentTeam)
            .Include(p => p.Game)
            .Where(p => p.GameId == query.GameId)
            .ConditionalWhere(p => p.TeamId == query.TeamId, query.TeamId.HasValue)
            .ConditionalWhere(p => p.TeamId == (query.HomeOrAway == HomeOrAway.Home ? p.Game.HomeTeamId : p.Game.AwayTeamId), !string.IsNullOrEmpty(query.HomeOrAway))
            .OrderBy(p => p.Player.JerseyNumber)
            .ToCollectionResultAsync();
    }       
    
    public async Task<CollectionResult<T>> GetAllAsync<T>(GetGoalieGameLogsQuery query)
        where T : IMappable<GoalieGameLog, T>
    {
        return await _context.GoalieGameLogs
            .AsNoTracking()
            .Include(p => p.Player)
            .Include(p => p.Team)
            .Include(p => p.OpponentTeam)
            .Include(p => p.Game)
            .Where(p => p.GameId == query.GameId)
            .ConditionalWhere(p => p.TeamId == query.TeamId, query.TeamId.HasValue)
            .ConditionalWhere(p => p.TeamId == (query.HomeOrAway == HomeOrAway.Home ? p.Game.HomeTeamId : p.Game.AwayTeamId), !string.IsNullOrEmpty(query.HomeOrAway))
            .OrderBy(p => p.Player.JerseyNumber)
            .ProjectTo<GoalieGameLog, T>()
            .ToCollectionResultAsync();
    }       
    
    public async Task<GoalieGameLog?> GetByIdAsync(int id)
    {
        return await _context.GoalieGameLogs
            .AsNoTracking()
            .Include(p => p.Player)
            .Include(p => p.Team)
            .Include(p => p.OpponentTeam)
            .Include(p => p.Game)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<T?> GetByIdAsync<T>(int id)
        where T : IMappable<GoalieGameLog, T>
    {
        return await _context.GoalieGameLogs
            .AsNoTracking()
            .Include(p => p.Player)
            .Include(p => p.Team)
            .Include(p => p.OpponentTeam)
            .Include(p => p.Game)
            .ProjectToFirstOrDefaultAsync<GoalieGameLog, T>(p => p.Id == id);
    }
}