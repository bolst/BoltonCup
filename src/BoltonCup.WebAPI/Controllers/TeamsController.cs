using BoltonCup.WebAPI.Dtos;
using BoltonCup.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BoltonCup.WebAPI.Controllers;

[Route("api/teams")]
[ApiController]
public class TeamsController(ITeamRepository _teams) : ControllerBase
{
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TeamDetailDto>>> Get([FromQuery] GetTeamsQuery query)
    {
        var teams = await _teams.GetAllAsync<TeamDetailDto>(query);
        return teams.ToList();
    }

    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<TeamDetailDto?>> Get(int id)
    {
        var result = await _teams.GetByIdAsync<TeamDetailDto>(id);
        if (result is null)
            return NotFound();
        return result;
    }
}
