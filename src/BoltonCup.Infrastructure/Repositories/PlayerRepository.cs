using BoltonCup.Infrastructure.Data;
using BoltonCup.Core;
using BoltonCup.Infrastructure.Extensions;
using BoltonCup.Core.Mappings;
using Microsoft.EntityFrameworkCore;

namespace BoltonCup.Infrastructure.Repositories;



public class PlayerRepository(BoltonCupDbContext _context) : IPlayerRepository
{
    public async Task<CollectionResult<Player>> GetAllAsync(GetPlayersQuery query)
    {
        return await _context.Players
            .AsNoTracking()
            .Include(p => p.Account)
            .Include(p => p.Tournament)
            .Include(p => p.Team)
            .ConditionalWhere(p => p.TournamentId == query.TournamentId, query.TournamentId.HasValue)
            .ConditionalWhere(p => p.TeamId == query.TeamId, query.TeamId.HasValue)
            .OrderBy(p => p.Id)
            .ToPaginatedListAsync(query);
    }       
    
    public async Task<CollectionResult<T>> GetAllAsync<T>(GetPlayersQuery query)
        where T : IMappable<Player, T>
    {
        return await _context.Players
            .AsNoTracking()
            .ConditionalWhere(p => p.TournamentId == query.TournamentId, query.TournamentId.HasValue)
            .ConditionalWhere(p => p.TeamId == query.TeamId, query.TeamId.HasValue)
            .OrderBy(p => p.Id)
            .ProjectTo<Player, T>()
            .ToPaginatedListAsync(query);
    }       
    
    public async Task<Player?> GetByIdAsync(int id)
    {
        return await _context.Players
            .AsNoTracking()
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

    public async Task<T?> GetByIdAsync<T>(int id)
        where T : IMappable<Player, T>
    {
        return await _context.Players
            .AsNoTracking()
            .ProjectToFirstOrDefaultAsync<Player, T>(p => p.Id == id);
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