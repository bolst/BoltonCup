using BoltonCup.Infrastructure.Data;
using BoltonCup.Core;
using BoltonCup.Infrastructure.Extensions;
using BoltonCup.Core.Values;
using Microsoft.EntityFrameworkCore;

namespace BoltonCup.Infrastructure.Repositories;



public class SkaterGameLogRepository(BoltonCupDbContext _context) : ISkaterGameLogRepository
{
    public async Task<IPagedList<SkaterGameLog>> GetAllAsync(GetSkaterGameLogsQuery query, CancellationToken cancellationToken = default)
    {
        return await _context.SkaterGameLogs
            .AsNoTracking()
            .Include(p => p.Player)
                .ThenInclude(p => p.Account)
            .Include(p => p.Team)
            .Where(p => p.GameId == query.GameId)
            .ConditionalWhere(p => p.TeamId == query.TeamId, query.TeamId.HasValue)
            .ConditionalWhere(p => p.Player.Position == query.Position, !string.IsNullOrEmpty(query.Position))
            .ConditionalWhere(p => p.TeamId == (query.HomeOrAway == HomeOrAway.Home ? p.Game.HomeTeamId : p.Game.AwayTeamId), !string.IsNullOrEmpty(query.HomeOrAway))
            .ApplySorting(query, x => x.OrderBy(p => p.Player.JerseyNumber))
            .ToPagedListAsync(query, cancellationToken: cancellationToken);
    }       
}