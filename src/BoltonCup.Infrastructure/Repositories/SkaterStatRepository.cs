using BoltonCup.Infrastructure.Data;
using BoltonCup.Core;
using BoltonCup.Infrastructure.Extensions;
using BoltonCup.Core.Mappings;
using Microsoft.EntityFrameworkCore;

namespace BoltonCup.Infrastructure.Repositories;



public class SkaterStatRepository(BoltonCupDbContext _context) : ISkaterStatRepository
{
    public async Task<CollectionResult<SkaterStat>> GetAllAsync(GetSkaterStatsQuery query)
    {
        return await _context.SkaterStats
            .AsNoTracking()
            .ConditionalWhere(p => p.TournamentId == query.TournamentId, query.TournamentId.HasValue)
            .ConditionalWhere(p => p.TeamId == query.TeamId, query.TeamId.HasValue)
            .ConditionalWhere(p => p.Position == query.Position, !string.IsNullOrEmpty(query.Position))
            .OrderBy(x => x.AccountId)
            .ThenByDescending(x => x.GameTime)
            .GroupBy(x => x.AccountId)
            .Select(g => new SkaterStat
            {
                PlayerId = g.First().PlayerId,
                GamesPlayed = g.Sum(x => x.GamesPlayed),
                Goals = g.Sum(x => x.Goals),
                Assists = g.Sum(x => x.Assists),
                Points = g.Sum(x => x.Points),
                PenaltyMinutes = g.Sum(x => x.PenaltyMinutes),
                AccountId = g.Key,
                FirstName = g.First().FirstName,
                LastName = g.First().LastName,
                Position = g.First().Position,
                JerseyNumber = g.First().JerseyNumber,
                Birthday = g.First().Birthday,
                ProfilePicture = g.First().ProfilePicture,
                TeamId = g.First().TeamId,
                TeamName = g.First().TeamName,
                TeamNameShort = g.First().TeamNameShort,
                TeamAbbreviation = g.First().TeamAbbreviation,
                TeamLogoUrl = g.First().TeamLogoUrl,
            })
            .ApplySorting(query, x => x
                .OrderByDescending(p => p.Points)
                .ThenByDescending(p => p.Goals)
                .ThenByDescending(p => p.Assists)
                .ThenBy(p => p.GamesPlayed)
            )
            .ToPaginatedListAsync(query);
    }       
    
    public async Task<CollectionResult<T>> GetAllAsync<T>(GetSkaterStatsQuery query)
        where T : IMappable<SkaterStat, T>
    {
        return await _context.SkaterStats
            .AsNoTracking()
            .ConditionalWhere(p => p.TournamentId == query.TournamentId, query.TournamentId.HasValue)
            .ConditionalWhere(p => p.TeamId == query.TeamId, query.TeamId.HasValue)
            .ConditionalWhere(p => p.Position == query.Position, !string.IsNullOrEmpty(query.Position))
            .OrderBy(x => x.AccountId)
            .ThenByDescending(x => x.GameTime)
            .GroupBy(x => x.AccountId)
            .Select(g => new SkaterStat
            {
                PlayerId = g.First().PlayerId,
                GamesPlayed = g.Sum(x => x.GamesPlayed),
                Goals = g.Sum(x => x.Goals),
                Assists = g.Sum(x => x.Assists),
                Points = g.Sum(x => x.Points),
                PenaltyMinutes = g.Sum(x => x.PenaltyMinutes),
                AccountId = g.Key,
                FirstName = g.First().FirstName,
                LastName = g.First().LastName,
                Position = g.First().Position,
                JerseyNumber = g.First().JerseyNumber,
                Birthday = g.First().Birthday,
                ProfilePicture = g.First().ProfilePicture,
                TeamId = g.First().TeamId,
                TeamName = g.First().TeamName,
                TeamNameShort = g.First().TeamNameShort,
                TeamAbbreviation = g.First().TeamAbbreviation,
                TeamLogoUrl = g.First().TeamLogoUrl,
            })
            .ApplySorting(query, x => x
                    .OrderByDescending(p => p.Points)
                    .ThenByDescending(p => p.Goals)
                    .ThenByDescending(p => p.Assists)
                    .ThenBy(p => p.GamesPlayed)
            )
            .ProjectTo<SkaterStat, T>()
            .ToPaginatedListAsync(query);
    }       
    
    public async Task<SkaterStat?> GetByIdAsync(int id)
    {
        return await _context.SkaterStats
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.PlayerId == id);
    }

    public async Task<T?> GetByIdAsync<T>(int id)
        where T : IMappable<SkaterStat, T>
    {
        return await _context.SkaterStats
            .AsNoTracking()
            .ProjectToFirstOrDefaultAsync<SkaterStat, T>(p => p.PlayerId == id);
    }
}