using BoltonCup.WebAPI.Dtos;
using BoltonCup.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BoltonCup.WebAPI.Controllers;

[Route("api/tournaments")]
[ApiController]
public class TournamentsController(ITournamentRepository _tournaments) : ControllerBase
{
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TournamentDetailDto>>> Get([FromQuery] GetTournamentsQuery query)
    {
        var result = await _tournaments.GetAllAsync(query, new TournamentDetailDto());
        return result.ToList();
    }

    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<TournamentDetailDto?>> Get(int id)
    {
        var result = await _tournaments.GetByIdAsync(id, new TournamentDetailDto());
        if (result is null)
            return NotFound();
        return result;
    }
    
}