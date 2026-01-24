using BoltonCup.WebAPI.Dtos;
using BoltonCup.Core;
using BoltonCup.Infrastructure.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BoltonCup.WebAPI.Controllers;

public class SkaterGameLogsController(ISkaterGameLogRepository _skaterGameLogs) : BoltonCupControllerBase
{
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<CollectionResult<SkaterGameLogDetailDto>>> GetSkaterGameLogs([FromQuery] GetSkaterGameLogsQuery query)
    {
        return Ok(await _skaterGameLogs.GetAllAsync<SkaterGameLogDetailDto>(query));
    }
}
