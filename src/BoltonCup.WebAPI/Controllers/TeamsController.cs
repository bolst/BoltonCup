using BoltonCup.Core;
using static BoltonCup.Infrastructure.Identity.BoltonCupRole;
using BoltonCup.WebAPI.Mapping;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BoltonCup.WebAPI.Controllers;

/// <summary>Manages team queries and team asset uploads.</summary>
public class TeamsController(ITeamRepository _teams, ITeamService _teamService, IMapper _mapper) : BoltonCupControllerBase
{
    /// <summary>Gets a paginated list of teams.</summary>
    /// <remarks>
    /// Gets a paginated list of teams.
    /// </remarks>
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<IPagedList<TeamDto>>> GetTeams([FromQuery] GetTeamsRequest request)
    {
        var query = _mapper.ToQuery(request);
        var teams = await _teams.GetAllAsync(query);
        return Ok(_mapper.ToDtoList(teams));
    }

    /// <summary>Gets a single team by its ID.</summary>
    /// <remarks>
    /// Gets a single team by its ID.
    /// </remarks>
    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<TeamSingleDto>> GetTeamById(int id)
    {
        var team = await _teams.GetByIdAsync(id);
        return OkOrNoContent(_mapper.ToDto(team));
    }

    /// <summary>Updates a team's logo using a pre-signed S3 key (admin only).</summary>
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
    
    /// <summary>Updates a team's banner using a pre-signed S3 key (admin only).</summary>
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
