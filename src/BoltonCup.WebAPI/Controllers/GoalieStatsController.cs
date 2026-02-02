using BoltonCup.WebAPI.Dtos;
using BoltonCup.Core;
using BoltonCup.Infrastructure.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BoltonCup.WebAPI.Controllers;

public class GoalieStatsController(IGoalieStatRepository _goalieStats) : BoltonCupControllerBase
{
    /// <remarks>
    /// Gets a paginated list of goalie statistics.
    /// </remarks>
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<PaginatedList<GoalieStatDetailDto>>> GetGoalieStats([FromQuery] GetGoalieStatsQuery query)
    {
        return Ok(await _goalieStats.GetAllAsync<GoalieStatDetailDto>(query));
    }
}
