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
        var result = await _tournaments.GetAllAsync(query);
        return result
            .Select(t => t.ToTournamentDetailDto())
            .ToList();
    }

    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<SingleTournamentDetailDto?>> Get(int id)
    {
        var result = await _tournaments.GetByIdAsync(id);
        if (result is null)
            return NotFound();
        return result.ToSingleTournamentDetailDto();
    }
    
    [AllowAnonymous]
    [HttpGet("active")]
    public async Task<ActionResult<TournamentDetailDto?>> GetActive()
    {
        var result = await _tournaments.GetActiveAsync();
        if (result is null)
            return NotFound();
        return result.ToSingleTournamentDetailDto();
    }
    
}