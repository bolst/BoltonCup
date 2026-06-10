using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace BoltonCup.WebAPI.Controllers;

/// <summary>Base controller that provides shared helpers for all BoltonCup API controllers.</summary>
[Route("api/[controller]")]
[ApiController]
public class BoltonCupControllerBase : ControllerBase
{
    private IMemoryCache Cache => HttpContext.RequestServices.GetRequiredService<IMemoryCache>();

    /// <summary>
    /// Gets a cached value by key, creating and caching it via <paramref name="factory"/> on a miss.
    /// Entries expire 5 minutes after creation unless <paramref name="duration"/> overrides it.
    /// </summary>
    protected Task<T?> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? duration = null)
        => Cache.GetOrCreateAsync(key, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = duration ?? TimeSpan.FromMinutes(5);
            return factory();
        });

    /// <summary>
    /// Returns Ok(result) if result is not null, otherwise returns NotFound().
    /// </summary>
    protected ActionResult<T> OkOrNoContent<T>(T? result)
        => result is null ? NoContent() : Ok(result);
}