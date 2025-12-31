using BoltonCup.WebAPI.Interfaces;
using BoltonCup.WebAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace BoltonCup.WebAPI.Controllers;

[Route("api/tournaments")]
[ApiController]
public class TournamentsController : ControllerBase
{
    private readonly ITournamentRepository _tournaments;

    public TournamentsController(ITournamentRepository tournaments)
    {
        _tournaments = tournaments;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Tournament>>> Get()
    {
        return Ok(await _tournaments.GetAllAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Tournament?>> Get(int id)
    {
        var result = await _tournaments.GetByIdAsync(id);
        if (result is null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] Tournament entity)
    {
        var result = await _tournaments.AddAsync(entity);
        if (!result)
        {
            return BadRequest();
        }

        return NoContent();
    }

    [HttpPut]
    public async Task<IActionResult> Put([FromBody] Tournament entity)
    {
        var result = await _tournaments.UpdateAsync(entity);
        if (!result)
        {
            return NotFound();
        }

        return NoContent();
    }
}