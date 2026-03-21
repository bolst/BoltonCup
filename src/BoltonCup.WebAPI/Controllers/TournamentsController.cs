using BoltonCup.Core;
using BoltonCup.WebAPI.Mapping;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BoltonCup.WebAPI.Controllers;

public class TournamentsController(ITournamentRepository _tournaments, ITournamentService _tournamentService, ITournamentMapper _mapper) 
    : BoltonCupControllerBase
{
    /// <remarks>
    /// Gets a paginated list of tournaments.
    /// </remarks>
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<IPagedList<TournamentDto>>> GetTournaments([FromQuery] GetTournamentsQuery query)
    {
        var tournaments = await _tournaments.GetAllAsync(query);
        return Ok(_mapper.ToDtoList(tournaments));
    }

    /// <remarks>
    /// Gets a single tournament by its ID.
    /// </remarks>
    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<TournamentSingleDto>> GetTournamentById(int id)
    {
        var tournament = await _tournaments.GetByIdAsync(id);
        return OkOrNotFound(_mapper.ToDto(tournament));
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