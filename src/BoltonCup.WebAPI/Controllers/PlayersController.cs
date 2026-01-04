using BoltonCup.WebAPI.Dtos;
using BoltonCup.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BoltonCup.WebAPI.Controllers;

[Route("api/players")]
[ApiController]
public class PlayersController(IPlayerRepository _players) : ControllerBase
{
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Player>>> Get(int? tournamentId = null)
    {
        return tournamentId is null 
            ? Ok(await _players.GetAllAsync())
            : Ok(await _players.GetAllAsync(tournamentId.Value));
    }

    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<SinglePlayerDetailDto?>> Get(int id)
    {
        var result = await _players.GetByIdAsync(id);
        if (result is null)
        {
            return NotFound();
        }
        var response = result.ToSinglePlayerDetailDto();
        return Ok(response);
    }
}
