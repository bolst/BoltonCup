using BoltonCup.Infrastructure.Data;
using BoltonCup.Infrastructure.Data.Entities;
using BoltonCup.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BoltonCup.Infrastructure.Repositories;

public class TournamentRepository : ITournamentRepository
{
    private readonly BoltonCupDbContext _context;

    public TournamentRepository(BoltonCupDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Tournament>> GetAllAsync()
    {
        return await _context.Tournaments
            .Include(e => e.Games)
            .Include(e => e.Teams)
            .OrderBy(t => t.StartDate)
            .ToListAsync();
    }
    
    public async Task<Tournament?> GetByIdAsync(int id)
    {
        return await _context.Tournaments
            .Include(e => e.Games)
            .Include(e => e.Teams)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<Tournament?> GetActiveAsync()
    {
        return await _context.Tournaments
            .Include(e => e.Games)
            .Include(e => e.Teams)
            .FirstOrDefaultAsync(e => e.IsActive);
    }

    public async Task<bool> AddAsync(Tournament entity)
    {
        await _context.Tournaments.AddAsync(entity);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<bool> UpdateAsync(Tournament entity)
    {
        _context.Tournaments.Update(entity);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _context.Teams.FindAsync(id);
        if (entity == null) return false;

        _context.Teams.Remove(entity);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }
}