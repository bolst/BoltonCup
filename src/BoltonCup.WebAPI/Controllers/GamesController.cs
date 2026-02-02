using BoltonCup.WebAPI.Dtos;
using BoltonCup.Core;
using BoltonCup.Infrastructure.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BoltonCup.WebAPI.Controllers;

public class GamesController(IGameRepository _games) : BoltonCupControllerBase
{
    /// <remarks>
    /// Gets a paginated list of games.
    /// </remarks>
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<PaginatedList<GameDetailDto>>> GetGames([FromQuery] GetGamesQuery query)
    {
        return Ok(await _games.GetAllAsync<GameDetailDto>(query));
    }

    /// <remarks>
    /// Gets a single game by its ID.
    /// </remarks>
    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<GameSingleDetailDto>> GetGameById(int id)
    {
        return OkOrNotFound(await _games.GetByIdAsync<GameSingleDetailDto>(id));
    }

    /// <remarks>
    /// Gets the box score for a game by its ID.
    /// </remarks>
    [AllowAnonymous]
    [HttpGet("{id:int}/boxscore")]
    public async Task<ActionResult<GameBoxScoreDto>> GetGameBoxScore(int id)
    {
        return OkOrNotFound(await _games.GetByIdAsync<GameBoxScoreDto>(id));
    }
}