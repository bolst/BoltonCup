using BoltonCup.WebAPI.Dtos;
using BoltonCup.Core;
using BoltonCup.Infrastructure.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BoltonCup.WebAPI.Controllers;

public class TeamsController(ITeamRepository _teams) : BoltonCupControllerBase
{
    /// <remarks>
    /// Gets a paginated list of teams.
    /// </remarks>
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<PaginatedList<TeamDetailDto>>> GetTeams([FromQuery] GetTeamsQuery query)
    {
        return Ok(await _teams.GetAllAsync<TeamDetailDto>(query));
    }

    /// <remarks>
    /// Gets a single team by its ID.
    /// </remarks>
    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<TeamSingleDetailDto>> GetTeamById(int id)
    {
        return OkOrNotFound(await _teams.GetByIdAsync<TeamSingleDetailDto>(id));
    }
}
