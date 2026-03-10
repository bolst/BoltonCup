using BoltonCup.WebAPI.Dtos;
using BoltonCup.Core;
using BoltonCup.Infrastructure.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BoltonCup.WebAPI.Controllers;

public class TournamentsController(ITournamentRepository _tournaments, ITournamentService _tournamentService) : BoltonCupControllerBase
{
    /// <remarks>
    /// Gets a paginated list of tournaments.
    /// </remarks>
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<PaginatedList<TournamentDetailDto>>> GetTournaments([FromQuery] GetTournamentsQuery query)
    {
        return Ok(await _tournaments.GetAllAsync<TournamentDetailDto>(query));
    }

    /// <remarks>
    /// Gets a single tournament by its ID.
    /// </remarks>
    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<TournamentSingleDetailDto>> GetTournamentById(int id)
    {
        return OkOrNotFound(await _tournaments.GetByIdAsync<TournamentSingleDetailDto>(id));
    }
    
    /// <remarks>
    /// Updates a tournament's logo by accepting a pre-signed S3 key.
    /// The client is responsible for uploading the image to S3 before calling this endpoint.
    /// </remarks>
    [HttpPut("{id:int}/logo")]
    public async Task<ActionResult> UpdateTournamentLogo(int id, string key)
    {
        await _tournamentService.UpdateLogoAsync(id, key);
        return Ok();
    }
}