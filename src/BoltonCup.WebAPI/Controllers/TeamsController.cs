using BoltonCup.WebAPI.Dtos;
using BoltonCup.Core;
using BoltonCup.Infrastructure.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BoltonCup.WebAPI.Controllers;

public class TeamsController(ITeamRepository _teams) : BoltonCupControllerBase
{
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<PaginatedList<TeamDetailDto>>> GetTeams([FromQuery] GetTeamsQuery query)
    {
        return Ok(await _teams.GetAllAsync<TeamDetailDto>(query));
    }

    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<TeamSingleDetailDto>> GetTeamById(int id)
    {
        return OkOrNotFound(await _teams.GetByIdAsync<TeamSingleDetailDto>(id));
    }
}
