using BoltonCup.Core;
using BoltonCup.WebAPI.Mapping;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BoltonCup.WebAPI.Controllers;

public class GoalieStatsController(IGoalieStatRepository _goalieStats, IGoalieStatMapper _mapper) : BoltonCupControllerBase
{
    /// <remarks>
    /// Gets a paginated list of goalie statistics.
    /// </remarks>
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<IPagedList<GoalieStatDto>>> GetGoalieStats([FromQuery] GetGoalieStatsRequest request)
    {
        var query = _mapper.ToQuery(request);
        var stats = await _goalieStats.GetAllAsync(query);
        return Ok(_mapper.ToDtoList(stats));
    }
}
