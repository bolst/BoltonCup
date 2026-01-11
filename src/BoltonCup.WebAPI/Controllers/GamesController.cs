using BoltonCup.WebAPI.Dtos;
using BoltonCup.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BoltonCup.WebAPI.Controllers;

public class GamesController(IGameRepository _games) : BoltonCupControllerBase
{
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<GameDetailDto>>> Get([FromQuery] GetGamesQuery query)
    {
        return Ok(await _games.GetAllAsync<GameDetailDto>(query));
    }

    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<GameSingleDetailDto?>> Get(int id)
    {
        var result = await _games.GetByIdAsync<GameSingleDetailDto>(id);
        if (result is null)
            return NotFound();
        return result;
    }
}