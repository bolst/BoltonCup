using BoltonCup.Infrastructure.Data;
using BoltonCup.Core;
using BoltonCup.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace BoltonCup.Infrastructure.Repositories;



public class PlayerRepository(BoltonCupDbContext _context) : IPlayerRepository
{
    public async Task<IPagedList<Player>> GetAllAsync(GetPlayersQuery query, CancellationToken cancellationToken = default)
    {
        return await _context.Players
            .AsNoTracking()
            .Include(p => p.Account)
            .Include(p => p.Tournament)
            .Include(p => p.Team)
            .ConditionalWhere(p => p.TournamentId == query.TournamentId, query.TournamentId.HasValue)
            .ConditionalWhere(p => p.TeamId == query.TeamId, query.TeamId.HasValue)
            .ApplySorting(query, x => x.OrderBy(p => p.Id))
            .ToPagedListAsync(query, cancellationToken: cancellationToken);
    }

    public async Task<Player?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var account = await _context.Accounts
            .AsNoTracking()
            .AsSplitQuery()
            .Include(a => a.Players)
                .ThenInclude(p => p.Tournament)
            .Include(a => a.Players)
                .ThenInclude(p => p.SkaterGameLogs)
            .Include(a => a.Players)
                .ThenInclude(p => p.GoalieGameLogs)
            .Include(a => a.Players)
                .ThenInclude(p => p.Goals)
            .Include(a => a.Players)
                .ThenInclude(p => p.PrimaryAssists)
            .Include(a => a.Players)
                .ThenInclude(p => p.SecondaryAssists)
            .Include(a => a.Players)
                .ThenInclude(p => p.Penalties)
            // home game goals
            .Include(a => a.Players)
                .ThenInclude(p => p.Team)
                .ThenInclude(t => t.HomeGames)
                .ThenInclude(g => g.Goals)
            // home game penalties
            .Include(a => a.Players)
                .ThenInclude(p => p.Team)
                .ThenInclude(t => t.HomeGames)
                .ThenInclude(g => g.Penalties)
            // home game tournament
            .Include(a => a.Players)
                .ThenInclude(p => p.Team)
                .ThenInclude(t => t.HomeGames)
                .ThenInclude(g => g.Tournament)
            // home game opponent
            .Include(a => a.Players)
                .ThenInclude(p => p.Team)
                .ThenInclude(t => t.HomeGames)
                .ThenInclude(g => g.AwayTeam)
            // away game goals
            .Include(a => a.Players)
                .ThenInclude(p => p.Team)
                .ThenInclude(t => t.AwayGames)
                .ThenInclude(g => g.Goals)
            // away game penalties
            .Include(a => a.Players)
                .ThenInclude(p => p.Team)
                .ThenInclude(t => t.AwayGames)
                .ThenInclude(g => g.Penalties)
            // away game tournament
            .Include(a => a.Players)
                .ThenInclude(p => p.Team)
                .ThenInclude(t => t.AwayGames)
                .ThenInclude(g => g.Tournament)
            // away game opponent
            .Include(a => a.Players)
                .ThenInclude(p => p.Team)
                .ThenInclude(t => t.AwayGames)
                .ThenInclude(g => g.HomeTeam)
            .FirstOrDefaultAsync(a => a.Players.Any(p => p.Id == id), cancellationToken: cancellationToken);
        return account?.Players.FirstOrDefault(p => p.Id == id);
    }
}