using BoltonCup.WebAPI.Dtos;
using BoltonCup.Core;
using BoltonCup.Infrastructure.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BoltonCup.WebAPI.Controllers;

public class GoalieGameLogsController(IGoalieGameLogRepository _goalieGameLogs) : BoltonCupControllerBase
{
    /// <remarks>
    /// Gets a collection of goalie game logs.
    /// </remarks>
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<CollectionResult<GoalieGameLogDetailDto>>> GetGoalieGameLogs([FromQuery] GetGoalieGameLogsQuery query)
    {
        return Ok(await _goalieGameLogs.GetAllAsync<GoalieGameLogDetailDto>(query));
    }
}
