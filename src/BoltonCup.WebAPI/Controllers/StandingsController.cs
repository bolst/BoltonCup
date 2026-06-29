using BoltonCup.Core;
using BoltonCup.WebAPI.Mapping;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BoltonCup.WebAPI.Controllers;

/// <summary>Provides computed tournament standings.</summary>
public class StandingsController(
    IStandingsService _standings,
    ITournamentRepository _tournaments,
    IMapper _mapper
) : BoltonCupControllerBase
{
    /// <summary>Gets the round robin and playoff standings for a tournament.</summary>
    /// <remarks>
    /// Gets the round robin and playoff standings for a tournament. When no tournament ID is
    /// supplied, the currently active tournament is used.
    /// </remarks>
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<TournamentStandingsDto>> GetStandings([FromQuery] int? tournamentId)
    {
        if (tournamentId is null)
        {
            var active = await _tournaments.GetActiveAsync();
            if (active is null)
                return NoContent();

            tournamentId = active.Id;
        }

        var standings = await GetOrCreateAsync($"{nameof(GetStandings)}:{tournamentId}", async () =>
        {
            var result = await _standings.GetStandingsAsync(tournamentId.Value);
            return _mapper.ToDto(result);
        });
        return Ok(standings);
    }
}