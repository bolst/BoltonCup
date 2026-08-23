using BoltonCup.Core;
using BoltonCup.WebAPI.Mapping;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BoltonCup.WebAPI.Controllers;

public class HighlightsController(
    IGameHighlightRepository _highlights,
    IMapper _mapper
) : BoltonCupControllerBase
{
    /// <remarks>
    /// Gets a paginated list of the most recent video highlights across all tournaments.
    /// </remarks>
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<IPagedList<RecentHighlightDto>>> GetHighlights([FromQuery] GetHighlightsRequest request)
    {
        var result = await GetOrCreateAsync(
            $"highlights:{request.Page}:{request.Size}:{request.SortBy}:{request.Descending}",
            async () =>
            {
                var query = _mapper.ToQuery(request);
                var highlights = await _highlights.GetAllAsync(query);
                return _mapper.ToDtoList(highlights);
            });
        return Ok(result);
    }
}
