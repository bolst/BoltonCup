using BoltonCup.Core;
using BoltonCup.Infrastructure.Data;
using BoltonCup.WebAPI.Mapping;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace BoltonCup.WebAPI.Controllers;

public class SystemController(
    BoltonCupDbContext _dbContext,
    IMemoryCache _cache
) : BoltonCupControllerBase
{
    /// <remarks>
    /// Gets the contextual configuration of the system (e.g., the active tournament ID).
    /// </remarks>
    [AllowAnonymous]
    [HttpGet("context")]
    [ResponseCache(Duration = 300)]
    public async Task<ActionResult<SystemContextDto>> GetSystemContext()
    {
        var context = await _cache.GetOrCreateAsync(nameof(GetSystemContext), async entry =>
        {
            var activeTournamentId = await _dbContext.Tournaments
                .Where(t => t.IsActive)
                .Select(t => t.Id)
                .FirstOrDefaultAsync();
            
            // TODO
            var statsTournamentId = 2;
            
            return new SystemContextDto(
                ActiveTournamentId: activeTournamentId,
                FeaturedStatsTournamentId: statsTournamentId
            );
        });
        
        return Ok(context);
    }
}

public record SystemContextDto(
    int? ActiveTournamentId,
    int? FeaturedStatsTournamentId
);