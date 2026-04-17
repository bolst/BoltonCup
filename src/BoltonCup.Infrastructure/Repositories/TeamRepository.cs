using BoltonCup.Infrastructure.Data;
using BoltonCup.Core;
using BoltonCup.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace BoltonCup.Infrastructure.Repositories;

public class TeamRepository(BoltonCupDbContext _context) : ITeamRepository
{
    public async Task<IPagedList<Team>> GetAllAsync(GetTeamsQuery query, CancellationToken cancellationToken = default)
    {
        return await _context.Teams
            .AsNoTracking()
            .Include(e => e.GeneralManager)
            .Include(e => e.Tournament)
            .Include(e => e.HomeGames)
            .Include(e => e.AwayGames)
            .Include(e => e.Players)
            .ThenInclude(p => p.Account)
            .ConditionalWhere(e => e.TournamentId == query.TournamentId, query.TournamentId.HasValue)
            .ApplySorting(query, x => x.OrderBy(e => e.Id))
            .ToPagedListAsync(query, cancellationToken: cancellationToken);
    }
    
    public async Task<Team?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Teams
            .AsNoTracking()
            .Include(e => e.GeneralManager)
            .Include(e => e.Tournament)
            .Include(e => e.HomeGames)
            .Include(e => e.AwayGames)
            .Include(e => e.Players).ThenInclude(p => p.Account)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken: cancellationToken);
    }
}