using BoltonCup.WebAPI.Interfaces;
using BoltonCup.WebAPI.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BoltonCup.WebAPI.Controllers;

[Route("api/teams")]
[ApiController]
public class TeamsController(ITeamRepository _teams) : ControllerBase
{
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TeamDetailDto>>> Get(int? tournamentId = null)
    {
        var teams = tournamentId is null 
            ? await _teams.GetAllAsync()
            : await _teams.GetAllAsync(tournamentId.Value);
        return Ok(teams.Select(t => t.ToTeamDetailDto()));
    }

    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<TeamDetailDto?>> Get(int id)
    {
        var result = await _teams.GetByIdAsync(id);
        if (result is null)
        {
            return NotFound();
        }

        var response = result.ToTeamDetailDto();
        return Ok(response);
    }
}
