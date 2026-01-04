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
    public async Task<ActionResult<IEnumerable<PlayerDetailDto>>> Get([FromQuery] GetPlayersQuery query)
    {
        var players = await _players.GetAllAsync(query, new PlayerDetailDto());
        return players.ToList();
    }

    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<PlayerDetailDto?>> Get(int id)
    {
        var result = await _players.GetByIdAsync(id, new PlayerDetailDto());
        if (result is null)
            return NotFound();
        return result;
    }
}
