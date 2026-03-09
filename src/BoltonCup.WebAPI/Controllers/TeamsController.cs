using BoltonCup.WebAPI.Dtos;
using BoltonCup.Core;
using BoltonCup.Infrastructure.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BoltonCup.WebAPI.Controllers;

public class TeamsController(ITeamRepository _teams, ITeamService _teamService) : BoltonCupControllerBase
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

    /// <remarks>
    /// Updates a team's logo by accepting a pre-signed S3 key for the new logo image.
    /// The client is responsible for uploading the image to S3 before calling this endpoint.
    /// </remarks>
    [HttpPut("{id:int}/logo")]
    public async Task<ActionResult> UpdateTeamLogo(int id, string key)
    {
        await _teamService.UpdateTeamLogoAsync(id, key);
        return Ok();
    }
    
    /// <remarks>
    /// Updates a team's logo by accepting a pre-signed S3 key for the new logo image.
    /// The client is responsible for uploading the image to S3 before calling this endpoint.
    /// </remarks>
    [HttpPut("{id:int}/banner")]
    public async Task<ActionResult> UpdateTeamBanner(int id, string key)
    {
        await _teamService.UpdateTeamBannerAsync(id, key);
        return Ok();
    }
}
