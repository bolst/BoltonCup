using Microsoft.AspNetCore.Mvc;

namespace BoltonCup.WebAPI.Controllers;

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