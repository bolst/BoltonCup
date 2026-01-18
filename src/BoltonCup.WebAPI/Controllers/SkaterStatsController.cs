using BoltonCup.WebAPI.Dtos;
using BoltonCup.Core;
using BoltonCup.Infrastructure.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BoltonCup.WebAPI.Controllers;

public class SkaterStatsController(ISkaterStatRepository _skaterStats) : BoltonCupControllerBase
{
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<PaginatedList<SkaterStatDetailDto>>> Get([FromQuery] GetSkaterStatsQuery query)
    {
        return Ok(await _skaterStats.GetAllAsync<SkaterStatDetailDto>(query));
    }

    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<SkaterStatDetailDto>> Get(int id)
    {
        return OkOrNotFound(await _skaterStats.GetByIdAsync<SkaterStatDetailDto>(id));
    }
}
