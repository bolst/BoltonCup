using BoltonCup.Core;
using BoltonCup.WebAPI.Mapping;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BoltonCup.WebAPI.Controllers;

public class SkaterGameLogsController(ISkaterGameLogRepository _skaterGameLogs, ISkaterGameLogMapper _mapper) : BoltonCupControllerBase
{
    /// <remarks>
    /// Gets a collection of skater game logs.
    /// </remarks>
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<IPagedList<SkaterGameLogDto>>> GetSkaterGameLogs([FromQuery] GetSkaterGameLogsRequest request)
    {
        var query = _mapper.ToQuery(request);
        var logs = await _skaterGameLogs.GetAllAsync(query);
        return Ok(_mapper.ToDtoList(logs));
    }
}
