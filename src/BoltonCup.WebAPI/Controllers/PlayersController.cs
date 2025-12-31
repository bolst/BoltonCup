using BoltonCup.WebAPI.Interfaces;
using BoltonCup.WebAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace BoltonCup.WebAPI.Controllers;

[Route("api/players")]
[ApiController]
public class PlayersController : ControllerBase
{
    private readonly IPlayerRepository _players;

    public PlayersController(IPlayerRepository players)
    {
        _players = players;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Player>>> Get(int? tournamentId = null)
    {
        return tournamentId is null 
            ? Ok(await _players.GetAllAsync())
            : Ok(await _players.GetAllAsync(tournamentId.Value));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Player?>> Get(Guid id)
    {
        var result = await _players.GetByIdAsync(id);
        if (result is null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] Player entity)
    {
        var result = await _players.AddAsync(entity);
        if (!result)
        {
            return BadRequest();
        }

        return NoContent();
    }

    [HttpPut]
    public async Task<IActionResult> Put([FromBody] Player entity)
    {
        var result = await _players.UpdateAsync(entity);
        if (!result)
        {
            return NotFound();
        }

        return NoContent();
    }
}
