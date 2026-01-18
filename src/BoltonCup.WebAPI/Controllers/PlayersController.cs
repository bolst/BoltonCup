using BoltonCup.WebAPI.Dtos;
using BoltonCup.Core;
using BoltonCup.Infrastructure.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BoltonCup.WebAPI.Controllers;

public class PlayersController(IPlayerRepository _players) : BoltonCupControllerBase
{
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<PaginatedList<PlayerDetailDto>>> GetPlayers([FromQuery] GetPlayersQuery query)
    {
        return Ok(await _players.GetAllAsync<PlayerDetailDto>(query));
    }

    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<PlayerSingleDetailDto>> GetPlayerById(int id)
    {
        return OkOrNotFound(await _players.GetByIdAsync<PlayerSingleDetailDto>(id));
    }
}
