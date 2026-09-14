using BoltonCup.Core;
using BoltonCup.WebAPI.Mapping;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace BoltonCup.WebAPI.Controllers;

/// <summary>Provides read access to published news posts.</summary>
public class NewsController(INewsPostService _news, IMapper _mapper) : BoltonCupControllerBase
{
    /// <summary>Gets a paginated list of published news posts, newest first.</summary>
    /// <remarks>
    /// Gets a paginated list of published news posts, newest first. Pass <c>label</c> to keep only
    /// posts tagged with that label, e.g. <c>Game Highlights</c>.
    /// </remarks>
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<IPagedList<NewsPostDto>>> GetNewsPosts([FromQuery] GetNewsPostsRequest request)
    {
        var label = request.Label?.Trim().ToLowerInvariant();
        var result = await GetOrCreateAsync(
            $"news:{request.Page}:{request.Size}:{request.SortBy}:{request.Descending}:{label}",
            async () =>
            {
                var query = _mapper.ToQuery(request);
                var posts = await _news.GetPublishedAsync(query, HttpContext.RequestAborted);
                return _mapper.ToDtoList(posts);
            });
        return Ok(result);
    }

    // Longer than any slug the generator produces; guards the cache against arbitrary keys.
    const int MaxSlugLength = 100;

    /// <summary>Gets a single published news post by its URL slug.</summary>
    /// <remarks>
    /// Gets a single published news post by its URL slug.
    /// </remarks>
    [AllowAnonymous]
    [HttpGet("{slug}")]
    public async Task<ActionResult<NewsPostSingleDto>> GetNewsPostBySlug(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug) || slug.Length > MaxSlugLength)
        {
            return NoContent();
        }

        // Slugs are stored lowercase; a capitalised link must still resolve.
        slug = slug.Trim().ToLowerInvariant();

        // Misses are not cached: a post published moments after a lookup must appear immediately.
        if (Cache.Get<NewsPostSingleDto>(CacheKey(slug)) is { } cached)
        {
            return Ok(cached);
        }

        var post = await _news.GetPublishedBySlugAsync(slug, HttpContext.RequestAborted);
        if (_mapper.ToDto(post) is not { } dto)
        {
            return NoContent();
        }

        Cache.Set(CacheKey(slug), dto, TimeSpan.FromMinutes(5));
        return Ok(dto);
    }

    static string CacheKey(string slug) => $"news:slug:{slug}";
}
