using BoltonCup.Core;
using BoltonCup.WebAPI.Mapping.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BoltonCup.WebAPI.Controllers;

public class GamesController(IGameRepository _games, IGameMapper _gameMapper) : BoltonCupControllerBase
{
    /// <remarks>
    /// Gets a paginated list of games.
    /// </remarks>
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<IPagedList<GameDto>>> GetGames([FromQuery] GetGamesQuery query)
    {
        var games = await _games.GetAllAsync(query);
        return Ok(_gameMapper.ToDtoList(games));
    }

    /// <remarks>
    /// Gets a single game by its ID.
    /// </remarks>
    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<GameSingleDto>> GetGameById(int id)
    {
        var game = await _games.GetByIdAsync(id);
        return OkOrNotFound(_gameMapper.ToDto(game));
    }
}