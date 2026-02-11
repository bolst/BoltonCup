using BoltonCup.WebAPI.Dtos;
using BoltonCup.Core;
using BoltonCup.Infrastructure.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BoltonCup.WebAPI.Controllers;

public class SkaterStatsController(ISkaterStatRepository _skaterStats) : BoltonCupControllerBase
{
    /// <remarks>
    /// Gets a paginated list of skater statistics.
    /// </remarks>
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<PaginatedList<SkaterStatDetailDto>>> GetSkaterStats([FromQuery] GetSkaterStatsQuery query)
    {
        return Ok(await _skaterStats.GetAllAsync<SkaterStatDetailDto>(query));
    }
}
