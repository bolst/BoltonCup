using BoltonCup.WebAPI.Data;
using BoltonCup.WebAPI.Interfaces;
using BoltonCup.WebAPI.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace BoltonCup.WebAPI.Repositories;



public class PlayerRepository(BoltonCupDbContext _context) : IPlayerRepository
{
    public async Task<IEnumerable<Player>> GetAllAsync()
    {
        return await _context.Players
            .Include(p => p.Account)
            .Include(p => p.Tournament)
            .Include(p => p.Team)
            .ToListAsync();
    }       
    
    public async Task<IEnumerable<Player>> GetAllAsync(int tournamentId)
    {
        return await _context.Players
            .Include(p => p.Account)
            .Include(p => p.Tournament)
            .Include(p => p.Team)
            .Where(t => t.TournamentId == tournamentId)
            .ToListAsync();
    }

    public async Task<Player?> GetByIdAsync(int id)
    {
        return await _context.Players
            .Include(p => p.Account)
            .Include(p => p.Tournament)
            .Include(p => p.Team).ThenInclude(t => t!.HomeGames).ThenInclude(g => g.AwayTeam)            
            .Include(p => p.Team).ThenInclude(t => t!.HomeGames).ThenInclude(g => g.Goals)
            .Include(p => p.Team).ThenInclude(t => t!.HomeGames).ThenInclude(g => g.Penalties)
            .Include(p => p.Team).ThenInclude(t => t!.AwayGames).ThenInclude(g => g.HomeTeam)             
            .Include(p => p.Team).ThenInclude(t => t!.AwayGames).ThenInclude(g => g.Goals)
            .Include(p => p.Team).ThenInclude(t => t!.AwayGames).ThenInclude(g => g.Penalties)
            .FirstOrDefaultAsync(p => p.Id == id);
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

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _context.Players.FindAsync(id);
        if (entity == null) return false;

        _context.Players.Remove(entity);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }
}