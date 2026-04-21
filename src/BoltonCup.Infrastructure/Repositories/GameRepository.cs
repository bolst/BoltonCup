using BoltonCup.Core;
using BoltonCup.Infrastructure.Data;
using BoltonCup.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace BoltonCup.Infrastructure.Repositories;



public class GameRepository(BoltonCupDbContext _context) : IGameRepository
{
    public async Task<IPagedList<Game>> GetAllAsync(GetGamesQuery query, CancellationToken cancellationToken = default)
    {
        return await _context.Games
            .AsNoTracking()
            .Include(p => p.Tournament)
            .Include(p => p.HomeTeam)
            .Include(p => p.AwayTeam)
            .Include(p => p.Goals)
            .ConditionalWhere(p => p.TournamentId == query.TournamentId, query.TournamentId.HasValue)
            .ConditionalWhere(p => p.HomeTeamId == query.TeamId || p.AwayTeamId == query.TeamId, query.TeamId.HasValue)
            .ApplySorting(query, x => x.OrderByDescending(p => p.Tournament.StartDate).ThenBy(p => p.GameTime))
            .ToPagedListAsync(query, cancellationToken: cancellationToken);
    }       
    
    public async Task<Game?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Games
            .AsNoTracking()
            .Include(p => p.Tournament)
            .Include(p => p.HomeTeam)
            .Include(p => p.AwayTeam)
            .Include(p => p.Goals)
                .ThenInclude(g => g.Scorer)
                .ThenInclude(p => p.Account)
            .Include(p => p.Goals)
                .ThenInclude(g => g.Assist1Player)
                .ThenInclude(p => p.Account)
            .Include(p => p.Goals)
                .ThenInclude(g => g.Assist2Player)
                .ThenInclude(p => p.Account)
            .Include(p => p.Penalties)
                .ThenInclude(x => x.Player)
                .ThenInclude(x => x.Account)
            .Include(p => p.Stars)
                .ThenInclude(s => s.Player)
                .ThenInclude(p => p.Account)
            .Include(p => p.Highlights.Take(3))
                .ThenInclude(h => h.Player)
                .ThenInclude(p => p.Account)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken: cancellationToken);
    }
}