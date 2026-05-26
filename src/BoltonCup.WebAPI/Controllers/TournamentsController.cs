using BoltonCup.Core;
using BoltonCup.WebAPI.Mapping;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using static BoltonCup.Infrastructure.Identity.BoltonCupRole;

namespace BoltonCup.WebAPI.Controllers;

/// <summary>Provides access to tournament data and admin tournament management.</summary>
public class TournamentsController(
    ITournamentRepository _tournaments,
    ITournamentService _tournamentService,
    IMapper _mapper
) : BoltonCupControllerBase
{
    /// <summary>Gets a paginated list of tournaments.</summary>
    /// <remarks>
    /// Gets a paginated list of tournaments.
    /// </remarks>
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<IPagedList<TournamentDto>>> GetTournaments([FromQuery] GetTournamentsRequest request)
    {
        var query = _mapper.ToQuery(request);
        var tournaments = await _tournaments.GetAllAsync(query);
        return Ok(_mapper.ToDtoList(tournaments));
    }

    /// <summary>Gets a single tournament by its ID.</summary>
    /// <remarks>
    /// Gets a single tournament by its ID.
    /// </remarks>
    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<TournamentSingleDto>> GetTournamentById(int id)
    {
        var tournament = await _tournaments.GetByIdAsync(id);
        return OkOrNoContent(_mapper.ToDto(tournament));
    }
    
    /// <summary>Updates a tournament's logo using a pre-signed S3 key (admin only).</summary>
    /// <remarks>
    /// Updates a tournament's logo by accepting a pre-signed S3 key.
    /// The client is responsible for uploading the image to S3 before calling this endpoint.
    /// </remarks>
    [Authorize(Roles = Admin)]
    [HttpPut("{id:int}/logo")]
    public async Task<ActionResult> UpdateTournamentLogo(int id, string key)
    {
        await _tournamentService.UpdateLogoAsync(id, key);
        return Ok();
    }
}