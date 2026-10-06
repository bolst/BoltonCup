using BoltonCup.Core;
using BoltonCup.WebAPI.Mapping;
using static BoltonCup.WebAPI.Auth.BoltonCupPolicy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace BoltonCup.WebAPI.Controllers;

/// <summary>Provides read access to published photo albums.</summary>
public class AlbumsController(IAlbumService _albums, IMapper _mapper) : BoltonCupControllerBase
{
    /// <summary>
    /// Gets a paginated list of images carrying the given tag, across published albums, newest
    /// album first.
    /// </summary>
    /// <remarks>
    /// Literal <c>images</c> outranks the <c>{slug}</c> route below it, so an album slugged
    /// <c>images</c> would never resolve via <see cref="GetAlbumBySlug"/>.
    /// </remarks>
    [AllowAnonymous]
    [HttpGet("images")]
    public async Task<ActionResult<IPagedList<AlbumImageDto>>> GetAlbumImages([FromQuery] GetAlbumImagesRequest request)
    {
        var result = await GetOrCreateAsync(
            $"albums:images:{request.TagType}:{request.TagTargetId}:{request.Page}:{request.Size}:{request.SortBy}:{request.Descending}",
            async () =>
            {
                var query = _mapper.ToQuery(request);
                var images = await _albums.GetPublishedImagesByTagAsync(query, HttpContext.RequestAborted);
                return _mapper.ToDtoList(images);
            });
        return Ok(result);
    }

    /// <summary>Copies a published image into a fresh temp key, so it can be staged as a new upload.</summary>
    /// <remarks>
    /// Returns the temp key, or <c>404</c> when the image does not exist or its album is not
    /// published. This is the only write path into storage that a client triggers indirectly
    /// through an id rather than a client-supplied key.
    /// </remarks>
    [Authorize(Policy = RequireCompletedAccount)]
    [HttpPost("images/{imageId:int}/stage")]
    public async Task<ActionResult<string>> StageAlbumImage(int imageId)
    {
        var tempKey = await _albums.StagePublishedImageAsync(imageId, HttpContext.RequestAborted);
        return tempKey is null ? NotFound() : Ok(tempKey);
    }

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