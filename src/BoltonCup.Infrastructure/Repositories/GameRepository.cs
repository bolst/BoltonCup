using BoltonCup.Core;
using BoltonCup.Infrastructure.Data;
using BoltonCup.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace BoltonCup.Infrastructure.Repositories;



public class GameRepository(BoltonCupDbContext _context) : IGameRepository
{
    public async Task<IPagedList<Game>> GetAllAsync(GetGamesQuery query)
    {
        return await _context.Games
            .AsNoTracking()
            .Include(p => p.Tournament)
            .Include(p => p.HomeTeam)
            .Include(p => p.AwayTeam)
            .Include(p => p.Goals)
            .ConditionalWhere(p => p.TournamentId == query.TournamentId, query.TournamentId.HasValue)
            .ConditionalWhere(p => p.HomeTeamId == query.TeamId || p.AwayTeamId == query.TeamId, query.TeamId.HasValue)
            .ApplySorting(query, x => x.OrderBy(p => p.GameTime))
            .ToPagedListAsync(query);
    }       
    
    public async Task<Game?> GetByIdAsync(int id)
    {
        return await _context.Games
            .AsNoTracking()
            .Include(p => p.Tournament)
            .Include(p => p.HomeTeam)
            .Include(p => p.AwayTeam)
            .Include(p => p.Goals)
                .ThenInclude(g => g.Scorer)
                .ThenInclude(p => p.Account)
            .Include(p => p.Goals)
                .ThenInclude(g => g.Assist1Player)
                .ThenInclude(p => p.Account)
            .Include(p => p.Goals)
                .ThenInclude(g => g.Assist2Player)
                .ThenInclude(p => p.Account)
            .Include(p => p.Penalties)
                .ThenInclude(x => x.Player)
                .ThenInclude(x => x.Account)
            .Include(p => p.Stars)
                .ThenInclude(s => s.Player)
                .ThenInclude(p => p.Account)
            .FirstOrDefaultAsync(p => p.Id == id);
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