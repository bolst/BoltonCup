using BoltonCup.Core;
using static BoltonCup.Infrastructure.Identity.BoltonCupRole;
using BoltonCup.WebAPI.Mapping;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BoltonCup.WebAPI.Controllers;

public class TeamsController(ITeamRepository _teams, ITeamService _teamService, ITeamMapper _teamMapper) : BoltonCupControllerBase
{
    /// <remarks>
    /// Gets a paginated list of teams.
    /// </remarks>
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<IPagedList<TeamDto>>> GetTeams([FromQuery] GetTeamsRequest request)
    {
        var query = _teamMapper.ToQuery(request);
        var teams = await _teams.GetAllAsync(query);
        return Ok(_teamMapper.ToDtoList(teams));
    }

    /// <remarks>
    /// Gets a single team by its ID.
    /// </remarks>
    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<TeamSingleDto>> GetTeamById(int id)
    {
        var team = await _teams.GetByIdAsync(id);
        return OkOrNoContent(_teamMapper.ToDto(team));
    }

    /// <remarks>
    /// Updates a team's logo by accepting a pre-signed S3 key.
    /// The client is responsible for uploading the image to S3 before calling this endpoint.
    /// </remarks>
    [Authorize(Roles = Admin)]
    [HttpPut("{id:int}/logo")]
    public async Task<ActionResult> UpdateTeamLogo(int id, string key)
    {
        await _teamService.UpdateLogoAsync(id, key);
        return Ok();
    }
    
    /// <remarks>
    /// Updates a team's banner by accepting a pre-signed S3 key.
    /// The client is responsible for uploading the image to S3 before calling this endpoint.
    /// </remarks>
    [Authorize(Roles = Admin)]
    [HttpPut("{id:int}/banner")]
    public async Task<ActionResult> UpdateTeamBanner(int id, string key)
    {
        await _teamService.UpdateBannerAsync(id, key);
        return Ok();
    }
}
