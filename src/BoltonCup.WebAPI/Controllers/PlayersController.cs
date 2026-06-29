using BoltonCup.Core;
using BoltonCup.WebAPI.Mapping;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BoltonCup.WebAPI.Controllers;

/// <summary>Provides read access to player information.</summary>
public class PlayersController(IPlayerRepository _players, IMapper _mapper) : BoltonCupControllerBase
{
    /// <summary>Gets a paginated list of players.</summary>
    /// <remarks>
    /// Gets a paginated list of players.
    /// </remarks>
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<IPagedList<PlayerDto>>> GetPlayers([FromQuery] GetPlayersRequest request)
    {
        var query = _mapper.ToQuery(request);
        var players = await _players.GetAllAsync(query);
        return Ok(_mapper.ToDtoList(players));
    }

    /// <summary>Gets a single player by its ID.</summary>
    /// <remarks>
    /// Gets a single player by its ID.
    /// </remarks>
    [AllowAnonymous]
    [HttpGet("{id:int}")]
    [ResponseCache(Duration = 300, VaryByQueryKeys = ["id"])]
    public async Task<ActionResult<PlayerSingleDto>> GetPlayerById(int id)
    {
        var player = await _players.GetByIdAsync(id);
        return OkOrNoContent(_mapper.ToDto(player));
    }
}