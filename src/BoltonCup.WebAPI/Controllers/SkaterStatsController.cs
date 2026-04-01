using BoltonCup.Core;
using BoltonCup.WebAPI.Filters;
using BoltonCup.WebAPI.Mapping;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BoltonCup.WebAPI.Controllers;

public class SkaterStatsController(
    ISkaterStatRepository _skaterStats, 
    ISkaterStatMapper _mapper,
    SkaterStatsFilterSchemaProvider _filterProvider
) : BoltonCupControllerBase
{
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

    [AllowAnonymous]
    [HttpGet("filter-schema")]
    public async Task<ActionResult<FilterSchemaDto>> GetSkaterStatFilterSchema()
        => Ok(_filterProvider.GetSchema());
}
