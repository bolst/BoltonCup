using BoltonCup.WebAPI.Data;
using BoltonCup.WebAPI.Interfaces;
using BoltonCup.WebAPI.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace BoltonCup.WebAPI.Repositories;

public class TeamRepository : ITeamRepository
{
    private readonly BoltonCupDbContext _context;

    public TeamRepository(BoltonCupDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Team>> GetAllAsync()
    {
        return await _context.Teams
            .OrderBy(t => t.TournamentId)
            .ThenBy(t => t.Id)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<Team>> GetAllAsync(int tournamentId)
    {
        return await _context.Teams
            .Where(t => t.TournamentId == tournamentId)
            .OrderBy(t => t.TournamentId)
            .ThenBy(t => t.Id)
            .ToListAsync();
    }

    public async Task<Team?> GetByIdAsync(int id)
    {
        return await _context.Teams.FindAsync(id);
    }

    public async Task<bool> AddAsync(Team entity)
    {
        await _context.Teams.AddAsync(entity);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<bool> UpdateAsync(Team entity)
    {
        _context.Teams.Update(entity);
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