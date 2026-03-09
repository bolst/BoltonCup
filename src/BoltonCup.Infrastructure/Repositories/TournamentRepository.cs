using BoltonCup.Infrastructure.Data;
using BoltonCup.Core;
using BoltonCup.Core.Mappings;
using BoltonCup.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace BoltonCup.Infrastructure.Repositories;

public class TournamentRepository(BoltonCupDbContext context) : ITournamentRepository
{
    public async Task<CollectionResult<Tournament>> GetAllAsync(GetTournamentsQuery query)
    {
        return await context.Tournaments
            .AsNoTracking()
            .Include(e => e.Games)
            .Include(e => e.Teams)
            .ApplySorting(query, x => x.OrderBy(t => t.StartDate))
            .ToPaginatedListAsync(query);
    }
        
    public async Task<CollectionResult<T>> GetAllAsync<T>(GetTournamentsQuery query)
        where T : IMappable<Tournament, T>
    {
        return await context.Tournaments
            .AsNoTracking()
            .ApplySorting(query, x => x.OrderBy(t => t.StartDate))
            .ProjectTo<Tournament, T>()
            .ToPaginatedListAsync(query);
    }
    
    public async Task<Tournament?> GetByIdAsync(int id)
    {
        return await context.Tournaments
            .AsNoTracking()
            .Include(e => e.Games)
            .Include(e => e.Teams)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<T?> GetByIdAsync<T>(int id)
        where T : IMappable<Tournament, T>
    {
        return await context.Tournaments
            .AsNoTracking()
            .ProjectToFirstOrDefaultAsync<Tournament, T>(e => e.Id == id);
    }

    public async Task<Tournament?> GetActiveAsync()
    {
        return await context.Tournaments
            .AsNoTracking()
            .Include(e => e.Games)
            .Include(e => e.Teams)
            .FirstOrDefaultAsync(e => e.IsActive);
    }

    public async Task<bool> AddAsync(Tournament entity)
    {
        await context.Tournaments.AddAsync(entity);
        var result = await context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<bool> UpdateAsync(Tournament entity)
    {
        context.Tournaments.Update(entity);
        var result = await context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await context.Teams.FindAsync(id);
        if (entity == null) return false;

        context.Teams.Remove(entity);
        var result = await context.SaveChangesAsync();
        return result > 0;
    }
}