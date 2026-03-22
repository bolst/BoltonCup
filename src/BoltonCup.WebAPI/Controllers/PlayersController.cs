using BoltonCup.Core;
using BoltonCup.WebAPI.Mapping;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BoltonCup.WebAPI.Controllers;

public class PlayersController(IPlayerRepository _players, IPlayerMapper _mapper) : BoltonCupControllerBase
{
    /// <remarks>
    /// Gets a paginated list of players.
    /// </remarks>
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<IPagedList<PlayerDto>>> GetPlayers([FromQuery] GetPlayersQuery query)
    {
        var players = await _players.GetAllAsync(query);
        return Ok(_mapper.ToDtoList(players));
    }

    /// <remarks>
    /// Gets a single player by its ID.
    /// </remarks>
    [AllowAnonymous]
    [HttpGet("{id:int}")]
    [ResponseCache(Duration = 300, VaryByQueryKeys = ["id"])]
    public async Task<ActionResult<PlayerSingleDto>> GetPlayerById(int id)
    {
        var player = await _players.GetByIdAsync(id);
        return OkOrNotFound(_mapper.ToDto(player));
    }
}
