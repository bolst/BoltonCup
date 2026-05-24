using BoltonCup.Core;
using BoltonCup.WebAPI.Mapping;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BoltonCup.WebAPI.Controllers;

/// <summary>Provides read access to skater statistics.</summary>
public class SkaterStatsController(
    ISkaterStatRepository _skaterStats,
    IMapper _mapper
) : BoltonCupControllerBase
{
    /// <summary>Gets a paginated list of skater statistics.</summary>
    /// <remarks>
    /// Gets a paginated list of skater statistics.
    /// </remarks>
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<IPagedList<SkaterStatDto>>> GetSkaterStats([FromQuery] GetSkaterStatsRequest request)
    {
        var query = _mapper.ToQuery(request);
        var stats = await _skaterStats.GetAllAsync(query);
        return Ok(_mapper.ToDtoList(stats));
    }
}
