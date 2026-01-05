using BoltonCup.Infrastructure.Data;
using BoltonCup.Core;
using BoltonCup.Core.Mappings;
using BoltonCup.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace BoltonCup.Infrastructure.Teams;

public class TeamRepository(BoltonCupDbContext _context) : ITeamRepository
{
    public async Task<IEnumerable<Team>> GetAllAsync(GetTeamsQuery query)
    {
        return await _context.Teams
            .Include(e => e.GeneralManager)
            .Include(e => e.Tournament)
            .Include(e => e.HomeGames)
            .Include(e => e.AwayGames)
            .Include(e => e.Players)
                .ThenInclude(p => p.Account)
            .ConditionalWhere(e => e.TournamentId == query.TournamentId, query.TournamentId.HasValue)
            .OrderBy(e => e.Id)
            .Page(query)
            .ToListAsync();
    }
        
    public async Task<IEnumerable<T>> GetAllAsync<T>(GetTeamsQuery query)
        where T : IMappable<Team, T>
    {
        return await _context.Teams
            .ConditionalWhere(e => e.TournamentId == query.TournamentId, query.TournamentId.HasValue)
            .OrderBy(e => e.Id)
            .Page(query)
            .ProjectTo<Team, T>()
            .ToListAsync();
    }
    
    public async Task<Team?> GetByIdAsync(int id)
    {
        return await _context.Teams
            .Include(e => e.GeneralManager)
            .Include(e => e.Tournament)
            .Include(e => e.HomeGames)
            .Include(e => e.AwayGames)
            .Include(e => e.Players).ThenInclude(p => p.Account)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<T?> GetByIdAsync<T>(int id)
        where T : IMappable<Team, T>
    {
        return await _context.Teams
            .Where(e => e.Id == id)
            .ProjectTo<Team, T>()
            .FirstOrDefaultAsync();
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