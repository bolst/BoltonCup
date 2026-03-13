using BoltonCup.Infrastructure.Data;
using BoltonCup.Core;
using BoltonCup.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace BoltonCup.Infrastructure.Repositories;

public class TournamentRepository(BoltonCupDbContext context) : ITournamentRepository
{
    public async Task<IPagedList<Tournament>> GetAllAsync(GetTournamentsQuery query)
    {
        return await context.Tournaments
            .AsNoTracking()
            .Include(e => e.Games)
            .Include(e => e.Teams)
            .ApplySorting(query, x => x.OrderBy(t => t.StartDate))
            .ToPagedListAsync(query);
    }
        
    public async Task<Tournament?> GetByIdAsync(int id)
    {
        return await context.Tournaments
            .AsNoTracking()
            .Include(e => e.Games)
            .Include(e => e.Teams)
            .FirstOrDefaultAsync(e => e.Id == id);
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