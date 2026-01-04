using BoltonCup.Infrastructure.Data;
using BoltonCup.Infrastructure.Data.Entities;
using BoltonCup.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BoltonCup.Infrastructure.Repositories;

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
            .Include(e => e.GeneralManager)
            .Include(e => e.Tournament)
            .Include(e => e.HomeGames)
            .Include(e => e.AwayGames)
            .Include(e => e.Players)
                .ThenInclude(p => p.Account)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<Team>> GetAllAsync(int tournamentId)
    {
        return await _context.Teams
            .Where(t => t.TournamentId == tournamentId)
            .Include(e => e.GeneralManager)
            .Include(e => e.Tournament)
            .Include(e => e.HomeGames)
            .Include(e => e.AwayGames)
            .Include(e => e.Players)
                .ThenInclude(p => p.Account)
            .ToListAsync();
    }

    public async Task<Team?> GetByIdAsync(int id)
    {
        return await _context.Teams
            .Include(e => e.GeneralManager)
            .Include(e => e.Tournament)
            .Include(e => e.HomeGames)
            .Include(e => e.AwayGames)
            .Include(e => e.Players)
                .ThenInclude(p => p.Account)
            .FirstOrDefaultAsync(e => e.Id == id);
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