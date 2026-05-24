using BoltonCup.Core;
using BoltonCup.WebAPI.Mapping;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BoltonCup.WebAPI.Controllers;

/// <summary>Provides read access to tournament games.</summary>
public class GamesController(IGameRepository _games, IGameMapper _gameMapper) : BoltonCupControllerBase
{
    /// <summary>Gets a paginated list of games.</summary>
    /// <remarks>
    /// Gets a paginated list of games.
    /// </remarks>
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<IPagedList<GameDto>>> GetGames([FromQuery] GetGamesRequest request)
    {
        var query = _gameMapper.ToQuery(request);
        var games = await _games.GetAllAsync(query);
        return Ok(_gameMapper.ToDtoList(games));
    }

    /// <summary>Gets a single game by its ID.</summary>
    /// <remarks>
    /// Gets a single game by its ID.
    /// </remarks>
    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<GameSingleDto>> GetGameById(int id)
    {
        var game = await _games.GetByIdAsync(id);
        return OkOrNoContent(_gameMapper.ToDto(game));
    }
}