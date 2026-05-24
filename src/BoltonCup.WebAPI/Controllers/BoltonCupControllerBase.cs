using Microsoft.AspNetCore.Mvc;

namespace BoltonCup.WebAPI.Controllers;

/// <summary>Base controller that provides shared helpers for all BoltonCup API controllers.</summary>
[Route("api/[controller]")]
[ApiController]
public class BoltonCupControllerBase : ControllerBase
{
    /// <summary>
    /// Returns Ok(result) if result is not null, otherwise returns NotFound().
    /// </summary>
    protected ActionResult<T> OkOrNoContent<T>(T? result)
        => result is null ? NoContent() : Ok(result);
}