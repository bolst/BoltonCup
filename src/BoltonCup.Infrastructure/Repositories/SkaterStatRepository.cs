using BoltonCup.Infrastructure.Data;
using BoltonCup.Core;
using BoltonCup.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace BoltonCup.Infrastructure.Repositories;



public class SkaterStatRepository(BoltonCupDbContext _context) : ISkaterStatRepository
{
    public async Task<IPagedList<SkaterStat>> GetAllAsync(GetSkaterStatsQuery query)
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
            .ToPagedListAsync(query);
    }       
}