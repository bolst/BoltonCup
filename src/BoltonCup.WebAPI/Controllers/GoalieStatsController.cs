using BoltonCup.WebAPI.Dtos;
using BoltonCup.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BoltonCup.WebAPI.Controllers;

public class GoalieStatsController(IGoalieStatRepository _goalieStats) : BoltonCupControllerBase
{
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<GoalieStatDetailDto>>> Get([FromQuery] GetGoalieStatsQuery query)
    {
        return Ok(await _goalieStats.GetAllAsync<GoalieStatDetailDto>(query));
    }

    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<GoalieStatDetailDto>> Get(int id)
    {
        return OkOrNotFound(await _goalieStats.GetByIdAsync<GoalieStatDetailDto>(id));
    }
}
