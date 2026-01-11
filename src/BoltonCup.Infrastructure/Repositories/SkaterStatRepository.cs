using BoltonCup.Infrastructure.Data;
using BoltonCup.Core;
using BoltonCup.Infrastructure.Extensions;
using BoltonCup.Core.Mappings;
using Microsoft.EntityFrameworkCore;

namespace BoltonCup.Infrastructure.Repositories;



public class SkaterStatRepository(BoltonCupDbContext _context) : ISkaterStatRepository
{
    public async Task<IEnumerable<SkaterStat>> GetAllAsync(GetSkaterStatsQuery query)
    {
        return await _context.SkaterStats
            .Include(p => p.Player)
            .Include(p => p.Tournament)
            .Include(p => p.Team)
            .ConditionalWhere(p => p.TournamentId == query.TournamentId, query.TournamentId.HasValue)
            .ConditionalWhere(p => p.TeamId == query.TeamId, query.TeamId.HasValue)
            .ConditionalWhere(p => p.Player.Position == query.Position, !string.IsNullOrEmpty(query.Position))
            .OrderByDescending(p => p.Points)
            .ThenByDescending(p => p.Goals)
            .ThenByDescending(p => p.Assists)
            .ThenBy(p => p.GamesPlayed)
            .Page(query)
            .ToListAsync();
    }       
    
    public async Task<IEnumerable<T>> GetAllAsync<T>(GetSkaterStatsQuery query)
        where T : IMappable<SkaterStat, T>
    {
        return await _context.SkaterStats
            .ConditionalWhere(p => p.TournamentId == query.TournamentId, query.TournamentId.HasValue)
            .ConditionalWhere(p => p.TeamId == query.TeamId, query.TeamId.HasValue)
            .ConditionalWhere(p => p.Player.Position == query.Position, !string.IsNullOrEmpty(query.Position))
            .OrderByDescending(p => p.Points)
            .ThenByDescending(p => p.Goals)
            .ThenByDescending(p => p.Assists)
            .ThenBy(p => p.GamesPlayed)
            .Page(query)
            .ProjectTo<SkaterStat, T>()
            .ToListAsync();
    }       
    
    public async Task<SkaterStat?> GetByIdAsync(int id)
    {
        return await _context.SkaterStats
            .Include(p => p.Player)
            .Include(p => p.Team)
            .Include(p => p.Tournament)
            .FirstOrDefaultAsync(p => p.PlayerId == id);
    }

    public async Task<T?> GetByIdAsync<T>(int id)
        where T : IMappable<SkaterStat, T>
    {
        return await _context.SkaterStats
            .Where(p => p.PlayerId == id)
            .ProjectTo<SkaterStat, T>()
            .FirstOrDefaultAsync();
    }
}