using BoltonCup.Core;
using BoltonCup.WebAPI.Mapping;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace BoltonCup.WebAPI.Controllers;

/// <summary>Provides read access to published photo albums.</summary>
public class AlbumsController(IAlbumService _albums, IMapper _mapper) : BoltonCupControllerBase
{
    /// <summary>Gets a paginated list of published albums, newest first.</summary>
    /// <remarks>
    /// Gets a paginated list of published albums, newest first. Pass <c>label</c> to keep only
    /// albums tagged with that label, e.g. <c>Game Highlights</c>.
    /// </remarks>
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<IPagedList<AlbumDto>>> GetAlbums([FromQuery] GetAlbumsRequest request)
    {
        var label = request.Label?.Trim().ToLowerInvariant();
        var result = await GetOrCreateAsync(
            $"albums:{request.Page}:{request.Size}:{request.SortBy}:{request.Descending}:{label}",
            async () =>
            {
                var query = _mapper.ToQuery(request);
                var albums = await _albums.GetPublishedAsync(query, HttpContext.RequestAborted);
                return _mapper.ToDtoList(albums);
            });
        return Ok(result);
    }

    const int MaxSlugLength = 100;

    /// <summary>Gets a single published album by its slug, including its photos.</summary>
    [AllowAnonymous]
    [HttpGet("{slug}")]
    public async Task<ActionResult<AlbumSingleDto>> GetAlbumBySlug(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug) || slug.Length > MaxSlugLength)
        {
            return NoContent();
        }

        slug = slug.Trim().ToLowerInvariant();
        if (Cache.Get<AlbumSingleDto>(CacheKey(slug)) is { } cached)
        {
            return Ok(cached);
        }

        var album = await _albums.GetPublishedBySlugAsync(slug, HttpContext.RequestAborted);
        if (_mapper.ToDto(album) is not { } dto)
        {
            return NoContent();
        }

        Cache.Set(CacheKey(slug), dto, DefaultCacheDuration);
        return Ok(dto);
    }

    static string CacheKey(string slug) => $"albums:slug:{slug}";
}