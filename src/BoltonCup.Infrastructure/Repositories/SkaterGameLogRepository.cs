using BoltonCup.Infrastructure.Data;
using BoltonCup.Core;
using BoltonCup.Infrastructure.Extensions;
using BoltonCup.Core.Mappings;
using BoltonCup.Core.Values;
using Microsoft.EntityFrameworkCore;

namespace BoltonCup.Infrastructure.Repositories;



public class SkaterGameLogRepository(BoltonCupDbContext _context) : ISkaterGameLogRepository
{
    public async Task<CollectionResult<SkaterGameLog>> GetAllAsync(GetSkaterGameLogsQuery query)
    {
        return await _context.SkaterGameLogs
            .AsNoTracking()
            .Include(p => p.Player)
            .Include(p => p.Team)
            .Include(p => p.OpponentTeam)
            .Include(p => p.Game)
            .Where(p => p.GameId == query.GameId)
            .ConditionalWhere(p => p.TeamId == query.TeamId, query.TeamId.HasValue)
            .ConditionalWhere(p => p.Player.Position == query.Position, !string.IsNullOrEmpty(query.Position))
            .ConditionalWhere(p => p.TeamId == (query.HomeOrAway == HomeOrAway.Home ? p.Game.HomeTeamId : p.Game.AwayTeamId), !string.IsNullOrEmpty(query.HomeOrAway))
            .OrderBy(p => p.Player.JerseyNumber)
            .ToCollectionResultAsync();
    }       
    
    public async Task<CollectionResult<T>> GetAllAsync<T>(GetSkaterGameLogsQuery query)
        where T : IMappable<SkaterGameLog, T>
    {
        return await _context.SkaterGameLogs
            .AsNoTracking()
            .Where(p => p.GameId == query.GameId)
            .ConditionalWhere(p => p.TeamId == query.TeamId, query.TeamId.HasValue)
            .ConditionalWhere(p => p.Player.Position == query.Position, !string.IsNullOrEmpty(query.Position))
            .ConditionalWhere(p => p.TeamId == (query.HomeOrAway == HomeOrAway.Home ? p.Game.HomeTeamId : p.Game.AwayTeamId), !string.IsNullOrEmpty(query.HomeOrAway))
            .OrderBy(p => p.Player.JerseyNumber)
            .ProjectTo<SkaterGameLog, T>()
            .ToCollectionResultAsync();
    }       
    
    public async Task<SkaterGameLog?> GetByIdAsync(int id)
    {
        return await _context.SkaterGameLogs
            .AsNoTracking()
            .Include(p => p.Player)
            .Include(p => p.Team)
            .Include(p => p.OpponentTeam)
            .Include(p => p.Game)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<T?> GetByIdAsync<T>(int id)
        where T : IMappable<SkaterGameLog, T>
    {
        return await _context.SkaterGameLogs
            .AsNoTracking()
            .ProjectToFirstOrDefaultAsync<SkaterGameLog, T>(p => p.Id == id);
    }
}