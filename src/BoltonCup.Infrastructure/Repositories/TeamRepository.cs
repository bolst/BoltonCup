using BoltonCup.Infrastructure.Data;
using BoltonCup.Core;
using BoltonCup.Core.Mappings;
using BoltonCup.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace BoltonCup.Infrastructure.Repositories;

public class TeamRepository(BoltonCupDbContext _context) : ITeamRepository
{
    public async Task<CollectionResult<Team>> GetAllAsync(GetTeamsQuery query)
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
            .OrderBy(e => e.Id)
            .ToPaginatedListAsync(query);
    }
        
    public async Task<CollectionResult<T>> GetAllAsync<T>(GetTeamsQuery query)
        where T : IMappable<Team, T>
    {
        return await _context.Teams
            .AsNoTracking()
            .ConditionalWhere(e => e.TournamentId == query.TournamentId, query.TournamentId.HasValue)
            .OrderBy(e => e.Id)
            .ProjectTo<Team, T>()
            .ToPaginatedListAsync(query);
    }
    
    public async Task<Team?> GetByIdAsync(int id)
    {
        return await _context.Teams
            .AsNoTracking()
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
            .AsNoTracking()
            .ProjectToFirstOrDefaultAsync<Team, T>(e => e.Id == id);
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