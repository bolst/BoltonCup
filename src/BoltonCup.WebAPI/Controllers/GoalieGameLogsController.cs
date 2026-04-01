using BoltonCup.Core;
using BoltonCup.WebAPI.Mapping;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BoltonCup.WebAPI.Controllers;

public class GoalieGameLogsController(IGoalieGameLogRepository _goalieGameLogs, IGoalieGameLogMapper _mapper) : BoltonCupControllerBase
{
    /// <remarks>
    /// Gets a collection of goalie game logs.
    /// </remarks>
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<IPagedList<GoalieGameLogDto>>> GetGoalieGameLogs([FromQuery] GetGoalieGameLogsRequest request)
    {
        var query = _mapper.ToQuery(request);
        var logs = await _goalieGameLogs.GetAllAsync(query);
        return Ok(_mapper.ToDtoList(logs));
    }
}
