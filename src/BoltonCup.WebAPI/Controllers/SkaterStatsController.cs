using BoltonCup.Core;
using BoltonCup.WebAPI.Mapping.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BoltonCup.WebAPI.Controllers;

public class SkaterStatsController(ISkaterStatRepository _skaterStats, ISkaterStatMapper _mapper) : BoltonCupControllerBase
{
    /// <remarks>
    /// Gets a paginated list of skater statistics.
    /// </remarks>
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<IPagedList<SkaterStatDto>>> GetSkaterStats([FromQuery] GetSkaterStatsQuery query)
    {
        var stats = await _skaterStats.GetAllAsync(query);
        return Ok(_mapper.ToDtoList(stats));
    }
}
