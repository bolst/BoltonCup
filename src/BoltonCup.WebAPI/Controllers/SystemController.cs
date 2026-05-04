using BoltonCup.Core;
using BoltonCup.WebAPI.Mapping;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace BoltonCup.WebAPI.Controllers;

public class SystemController(
    ITournamentRepository _tournamentRepo,
    ISkaterStatRepository _skaterStatRepo,
    IGoalieStatRepository _goalieStatRepo,
    ITournamentMapper _tournamentMapper,
    IMemoryCache _cache
) : BoltonCupControllerBase
{
    /// <remarks>
    /// Gets the contextual configuration of the system (e.g., active tournament and featured stats).
    /// </remarks>
    [AllowAnonymous]
    [HttpGet("context")]
    [ResponseCache(Duration = 300)]
    public async Task<ActionResult<SystemContextDto>> GetSystemContext()
    {
        var context = await _cache.GetOrCreateAsync(nameof(GetSystemContext), async entry =>
        {
            var activeTournament = await _tournamentRepo.GetActiveAsync();
            var featuredStats = await GetFeaturedStats();
            
            return new SystemContextDto(
                ActiveTournament: _tournamentMapper.ToDto(activeTournament),
                FeaturedStats: featuredStats
            );
        });
        
        return Ok(context);
    }


    private async Task<TournamentStatLeadersDto> GetFeaturedStats()
    {
        // TODO
        var statsTournamentId = 2;
        var statsTitle = "2025 Leaders";
        var baseSkaterQuery = new GetSkaterStatsQuery { TournamentId = statsTournamentId, Size = 5, Descending = true };
        var baseGoalieQuery = new GetGoalieStatsQuery { TournamentId = statsTournamentId, Size = 5 };
            
        var points = await _skaterStatRepo.GetAllAsync(baseSkaterQuery with { SortBy = nameof(SkaterStat.Points)});
        var goals = await _skaterStatRepo.GetAllAsync(baseSkaterQuery with { SortBy = nameof(SkaterStat.Goals)});
        var pims = await _skaterStatRepo.GetAllAsync(baseSkaterQuery with { SortBy = nameof(SkaterStat.PenaltyMinutes)});
        var gaa = await _goalieStatRepo.GetAllAsync(baseGoalieQuery with { SortBy = nameof(GoalieStat.GoalsAgainstAverage)});

        return new TournamentStatLeadersDto
        {
            TournamentId = statsTournamentId,
            Title = statsTitle,
            StatLeaders =
            [
                _tournamentMapper.ToDto(
                    "Points", 
                    points.Items,
                    x => x.Points
                ),
                _tournamentMapper.ToDto(
                    "Goals", 
                    goals.Items,
                    x => x.Goals
                ),
                _tournamentMapper.ToDto(
                    "PIM", 
                    pims.Items,
                    x => x.PenaltyMinutes,
                    "N0"
                ),
                _tournamentMapper.ToDto(
                    "GAA", 
                    gaa.Items,
                    x => x.GoalsAgainstAverage,
                    "F2"
                ),
            ]
        };
    }
}

public record SystemContextDto(
    TournamentSingleDto? ActiveTournament,
    TournamentStatLeadersDto? FeaturedStats
);

public record TournamentStatLeadersDto
{
    public required int TournamentId { get; init; }
    
    public required string Title { get; init; }

    public IEnumerable<PlayerStatLeadersDto> StatLeaders { get; init; } = [];
}