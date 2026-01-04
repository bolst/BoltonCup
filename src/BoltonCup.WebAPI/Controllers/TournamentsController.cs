using BoltonCup.WebAPI.Dtos;
using BoltonCup.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BoltonCup.WebAPI.Controllers;

[Route("api/tournaments")]
[ApiController]
public class TournamentsController(ITournamentRepository _tournaments) : ControllerBase
{
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TournamentDetailDto>>> Get()
    {
        var result = await _tournaments.GetAllAsync();
        var response = result.Select(t => t.ToTournamentDetailDto());
        return Ok(response);
    }

    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<TournamentDetailDto?>> Get(int id)
    {
        var result = await _tournaments.GetByIdAsync(id);
        if (result is null)
        {
            return NotFound();
        }
        var response = result.ToTournamentDetailDto();
        return Ok(response);
    }
    
    [AllowAnonymous]
    [HttpGet("active")]
    public async Task<ActionResult<TournamentDetailDto?>> GetActive()
    {
        var result = await _tournaments.GetActiveAsync();
        if (result is null)
        {
            return NotFound();
        }
        var response = result.ToTournamentDetailDto();
        return Ok(response);
    }
    
}