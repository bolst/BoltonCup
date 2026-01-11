using BoltonCup.WebAPI.Dtos;
using BoltonCup.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BoltonCup.WebAPI.Controllers;

public class TournamentsController(ITournamentRepository _tournaments) : BoltonCupControllerBase
{
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TournamentDetailDto>>> Get([FromQuery] GetTournamentsQuery query)
    {
        return Ok(await _tournaments.GetAllAsync<TournamentDetailDto>(query));
    }

    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<TournamentSingleDetailDto?>> Get(int id)
    {
        var result = await _tournaments.GetByIdAsync<TournamentSingleDetailDto>(id);
        if (result is null)
            return NotFound();
        return result;
    }
    
}