using BoltonCup.Infrastructure.Data;
using BoltonCup.Core;
using BoltonCup.Infrastructure.Extensions;
using BoltonCup.Core.Values;
using Microsoft.EntityFrameworkCore;

namespace BoltonCup.Infrastructure.Repositories;



public class GoalieGameLogRepository(BoltonCupDbContext _context) : IGoalieGameLogRepository
{
    public async Task<IPagedList<GoalieGameLog>> GetAllAsync(GetGoalieGameLogsQuery query, CancellationToken cancellationToken = default)
    {
        return await _context.GoalieGameLogs
            .AsNoTracking()
            .Include(g => g.Player)
                .ThenInclude(p => p.Account)
            .Include(g => g.Team)
            .Include(g => g.OpponentTeam)
            .Include(g => g.Game)
            .Where(g => g.GameId == query.GameId)
            .ConditionalWhere(g => g.TeamId == query.TeamId, query.TeamId.HasValue)
            .ConditionalWhere(g => g.TeamId == (query.HomeOrAway == HomeOrAway.Home ? g.Game.HomeTeamId : g.Game.AwayTeamId), !string.IsNullOrEmpty(query.HomeOrAway))
            .ApplySorting(query, x => x.OrderBy(p => p.Player.JerseyNumber))
            .ToPagedListAsync(query, cancellationToken: cancellationToken);
    }       
}