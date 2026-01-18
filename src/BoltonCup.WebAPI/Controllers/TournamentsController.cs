using BoltonCup.WebAPI.Dtos;
using BoltonCup.Core;
using BoltonCup.Infrastructure.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BoltonCup.WebAPI.Controllers;

public class TournamentsController(ITournamentRepository _tournaments) : BoltonCupControllerBase
{
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<PaginatedList<TournamentDetailDto>>> GetTournaments([FromQuery] GetTournamentsQuery query)
    {
        return Ok(await _tournaments.GetAllAsync<TournamentDetailDto>(query));
    }

    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<TournamentSingleDetailDto>> GetTournamentById(int id)
    {
        return OkOrNotFound(await _tournaments.GetByIdAsync<TournamentSingleDetailDto>(id));
    }
    
}