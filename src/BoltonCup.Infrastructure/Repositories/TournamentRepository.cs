using BoltonCup.Infrastructure.Data;
using BoltonCup.Core;
using BoltonCup.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace BoltonCup.Infrastructure.Repositories;

public class TournamentRepository(BoltonCupDbContext context) : ITournamentRepository
{
    public async Task<IPagedList<Tournament>> GetAllAsync(GetTournamentsQuery query, CancellationToken cancellationToken = default)
    {
        return await context.Tournaments
            .AsNoTracking()
            .ConditionalWhere(e => e.IsRegistrationOpen == query.RegistrationOpen!.Value, query.RegistrationOpen.HasValue)
            .Include(e => e.Games)
            .Include(e => e.Teams)
            .Include(e => e.Gallery)
            .ApplySorting(query, x => x.OrderBy(t => t.StartDate))
            .ToPagedListAsync(query, cancellationToken: cancellationToken);
    }
        
    public async Task<Tournament?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await context.Tournaments
            .AsNoTracking()
            .Include(e => e.InfoGuide)
            .Include(e => e.Games)
            .Include(e => e.Teams)
            .Include(e => e.Gallery)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken: cancellationToken);
    }
}