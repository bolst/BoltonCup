using BoltonCup.WebAPI.Data;
using BoltonCup.WebAPI.Interfaces;
using BoltonCup.WebAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace BoltonCup.WebAPI.Repositories;

public class PlayerRepository : IPlayerRepository
{
    private readonly BoltonCupDbContext _context;

    public PlayerRepository(BoltonCupDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Player>> GetAllAsync()
    {
        return await _context.Players
            .OrderBy(t => t.TournamentId)
            .ThenBy(t => t.Id)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<Player>> GetAllAsync(int tournamentId)
    {
        return await _context.Players
            .Where(t => t.TournamentId == tournamentId)
            .OrderBy(t => t.Id)
            .ToListAsync();
    }

    public async Task<Player?> GetByIdAsync(Guid id)
    {
        return await _context.Players.FindAsync(id);
    }

    public async Task<bool> AddAsync(Player entity)
    {
        await _context.Players.AddAsync(entity);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<bool> UpdateAsync(Player entity)
    {
        _context.Players.Update(entity);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var entity = await _context.Players.FindAsync(id);
        if (entity == null) return false;

        _context.Players.Remove(entity);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }
}