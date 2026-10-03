using BoltonCup.Core;
using BoltonCup.WebAPI.Mapping;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace BoltonCup.WebAPI.Controllers;

/// <summary>Provides read access to franchises: the persistent club identities above each season's team.</summary>
public class FranchisesController(IFranchiseService _franchiseService, IMapper _mapper) : BoltonCupControllerBase
{
    /// <summary>Gets all franchises with their title and season counts.</summary>
    /// <remarks>
    /// Gets all franchises with their current brand, title count and season count, sorted by titles descending,
    /// then by name. Results are cached.
    /// </remarks>
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<FranchiseDto>>> GetFranchises()
    {
        var franchises = await GetOrCreateAsync(nameof(GetFranchises), async () =>
        {
            var result = await _franchiseService.GetAllAsync(HttpContext.RequestAborted);
            return _mapper.ToDtoList(result);
        });
        return Ok(franchises);
    }

    // Longer than any slug the generator produces; guards the cache against arbitrary keys.
    const int MaxSlugLength = 100;

    /// <summary>Gets a single franchise by its URL slug.</summary>
    /// <remarks>
    /// Gets a single franchise with its owners, titles, all-time record, season history and skater and goalie
    /// leaders. Returns no content when no franchise has this slug.
    /// </remarks>
    [AllowAnonymous]
    [HttpGet("{slug}")]
    public async Task<ActionResult<FranchiseSingleDto>> GetFranchiseBySlug(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug) || slug.Length > MaxSlugLength)
        {
            return NoContent();
        }

        // Slugs are stored lowercase; a capitalised link must still resolve.
        slug = slug.Trim().ToLowerInvariant();

        // Misses are not cached, so a franchise created or renamed shows up without waiting for expiry.
        if (Cache.Get<FranchiseSingleDto>(CacheKey(slug)) is { } cached)
        {
            return Ok(cached);
        }

        var result = await _franchiseService.GetDetailBySlugAsync(slug, HttpContext.RequestAborted);
        if (_mapper.ToDto(result) is not { } franchise)
        {
            return NoContent();
        }

        Cache.Set(CacheKey(slug), franchise, DefaultCacheDuration);
        return Ok(franchise);
    }

    static string CacheKey(string slug) => $"franchises:slug:{slug}";
}