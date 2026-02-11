using BoltonCup.Core;
using BoltonCup.Core.Mappings;
using BoltonCup.Infrastructure.Data;
using BoltonCup.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace BoltonCup.Infrastructure.Repositories;



public class GameRepository(BoltonCupDbContext _context) : IGameRepository
{
    public async Task<CollectionResult<Game>> GetAllAsync(GetGamesQuery query)
    {
        return await _context.Games
            .AsNoTracking()
            .Include(p => p.Tournament)
            .Include(p => p.HomeTeam)
            .Include(p => p.AwayTeam)
            .Include(p => p.Goals)
            .Include(p => p.Penalties)
            .ConditionalWhere(p => p.TournamentId == query.TournamentId, query.TournamentId.HasValue)
            .ConditionalWhere(p => p.HomeTeamId == query.TeamId || p.AwayTeamId == query.TeamId, query.TeamId.HasValue)
            .OrderBy(p => p.GameTime)
            .ToPaginatedListAsync(query);
    }       
    
    public async Task<CollectionResult<T>> GetAllAsync<T>(GetGamesQuery query)
        where T : IMappable<Game, T>
    {
        return await _context.Games
            .AsNoTracking()
            .ConditionalWhere(p => p.TournamentId == query.TournamentId, query.TournamentId.HasValue)
            .ConditionalWhere(p => p.HomeTeamId == query.TeamId || p.AwayTeamId == query.TeamId, query.TeamId.HasValue)
            .OrderBy(p => p.GameTime)
            .ProjectTo<Game, T>()
            .ToPaginatedListAsync(query);
    }       
    
    public async Task<Game?> GetByIdAsync(int id)
    {
        return await _context.Games
            .AsNoTracking()
            .Include(p => p.Tournament)
            .Include(p => p.HomeTeam)
            .Include(p => p.AwayTeam)
            .Include(p => p.Goals)
            .Include(p => p.Penalties)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<T?> GetByIdAsync<T>(int id)
        where T : IMappable<Game, T>
    {
        return await _context.Games
            .AsNoTracking()
            .ProjectToFirstOrDefaultAsync<Game, T>(p => p.Id == id);
    }

    public async Task<bool> AddAsync(Game entity)
    {
        await _context.Games.AddAsync(entity);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<bool> UpdateAsync(Game entity)
    {
        _context.Games.Update(entity);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _context.Games.FindAsync(id);
        if (entity == null) return false;

        _context.Games.Remove(entity);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }
}