using BoltonCup.Core;
using BoltonCup.WebAPI.Mapping;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static BoltonCup.Infrastructure.Identity.BoltonCupRole;

namespace BoltonCup.WebAPI.Controllers;

public class GamesController(
    IGameRepository _games,
    ISkaterStatRepository _skaterStats,
    IMapper _mapper,
    IGameWriteService _gameWrites,
    IMusicLibraryService _music
) : BoltonCupControllerBase
{
    /// <remarks>
    /// Gets a paginated list of games.
    /// </remarks>
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<IPagedList<GameDto>>> GetGames([FromQuery] GetGamesRequest request)
    {
        var query = _mapper.ToQuery(request);
        var games = await _games.GetAllAsync(query);
        return Ok(_mapper.ToDtoList(games));
    }

    /// <remarks>
    /// Gets a single game by its ID.
    /// </remarks>
    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<GameSingleDto>> GetGameById(int id)
    {
        var game = await _games.GetByIdAsync(id);
        if (game is null)
        {
            return NoContent();
        }
        var homeStats = await _skaterStats.GetCareerStatsAsync(game.TournamentId, game.HomeTeamId);
        var awayStats = await _skaterStats.GetCareerStatsAsync(game.TournamentId, game.AwayTeamId);
        return Ok(_mapper.ToDto(game, homeStats, awayStats));
    }

    /// <remarks>Updates a game's state (timekeeper or admin only).</remarks>
    [Authorize(Roles = $"{Admin},{Timekeeper}")]
    [HttpPatch("{id:int}/state")]
    public async Task<IActionResult> UpdateGameState(int id, [FromBody] UpdateGameStateRequest request)
    {
        await _gameWrites.UpdateStateAsync(_mapper.ToCommand(id, request));
        return Ok();
    }

    /// <remarks>
    /// Gets the computed music playlist for a game: matched player song requests first, then the tournament
    /// base pool, plus any requests with no uploaded file (timekeeper or admin only).
    /// </remarks>
    [Authorize(Roles = $"{Admin},{Timekeeper}")]
    [HttpGet("{id:int}/playlist")]
    public async Task<ActionResult<GamePlaylistDto>> GetGamePlaylist(int id)
    {
        var result = await _music.GetGamePlaylistAsync(id);
        return Ok(_mapper.ToDto(result));
    }

    /// <remarks>Records a goal in a game (timekeeper or admin only).</remarks>
    [Authorize(Roles = $"{Admin},{Timekeeper}")]
    [HttpPost("{id:int}/goals")]
    public async Task<ActionResult<int>> AddGoal(int id, [FromBody] CreateGoalRequest request)
    {
        var goalId = await _gameWrites.AddGoalAsync(_mapper.ToCommand(id, request));
        return Ok(goalId);
    }

    /// <remarks>Updates an existing goal (timekeeper or admin only).</remarks>
    [Authorize(Roles = $"{Admin},{Timekeeper}")]
    [HttpPut("{id:int}/goals/{goalId:int}")]
    public async Task<IActionResult> UpdateGoal(int id, int goalId, [FromBody] UpdateGoalRequest request)
    {
        await _gameWrites.UpdateGoalAsync(_mapper.ToCommand(id, goalId, request));
        return Ok();
    }

    /// <remarks>Deletes a goal (timekeeper or admin only).</remarks>
    [Authorize(Roles = $"{Admin},{Timekeeper}")]
    [HttpDelete("{id:int}/goals/{goalId:int}")]
    public async Task<IActionResult> DeleteGoal(int id, int goalId)
    {
        await _gameWrites.DeleteGoalAsync(id, goalId);
        return Ok();
    }

    /// <remarks>Records a penalty in a game (timekeeper or admin only).</remarks>
    [Authorize(Roles = $"{Admin},{Timekeeper}")]
    [HttpPost("{id:int}/penalties")]
    public async Task<ActionResult<int>> AddPenalty(int id, [FromBody] CreatePenaltyRequest request)
    {
        var penaltyId = await _gameWrites.AddPenaltyAsync(_mapper.ToCommand(id, request));
        return Ok(penaltyId);
    }

    /// <remarks>Updates an existing penalty (timekeeper or admin only).</remarks>
    [Authorize(Roles = $"{Admin},{Timekeeper}")]
    [HttpPut("{id:int}/penalties/{penaltyId:int}")]
    public async Task<IActionResult> UpdatePenalty(int id, int penaltyId, [FromBody] UpdatePenaltyRequest request)
    {
        await _gameWrites.UpdatePenaltyAsync(_mapper.ToCommand(id, penaltyId, request));
        return Ok();
    }

    /// <remarks>Deletes a penalty (timekeeper or admin only).</remarks>
    [Authorize(Roles = $"{Admin},{Timekeeper}")]
    [HttpDelete("{id:int}/penalties/{penaltyId:int}")]
    public async Task<IActionResult> DeletePenalty(int id, int penaltyId)
    {
        await _gameWrites.DeletePenaltyAsync(id, penaltyId);
        return Ok();
    }

    /// <remarks>Replaces the 1st/2nd/3rd stars for a game (timekeeper or admin only).</remarks>
    [Authorize(Roles = $"{Admin},{Timekeeper}")]
    [HttpPut("{id:int}/stars")]
    public async Task<IActionResult> SetGameStars(int id, [FromBody] SetGameStarsRequest request)
    {
        await _gameWrites.SetStarsAsync(_mapper.ToCommand(id, request));
        return Ok();
    }
}