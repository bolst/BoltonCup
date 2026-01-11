using BoltonCup.WebAPI.Dtos;
using BoltonCup.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BoltonCup.WebAPI.Controllers;

public class PlayersController(IPlayerRepository _players) : BoltonCupControllerBase
{
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PlayerDetailDto>>> Get([FromQuery] GetPlayersQuery query)
    {
        return Ok(await _players.GetAllAsync<PlayerDetailDto>(query));
    }

    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<PlayerSingleDetailDto?>> Get(int id)
    {
        var result = await _players.GetByIdAsync<PlayerSingleDetailDto>(id);
        if (result is null)
            return NotFound();
        return result;
    }
}
