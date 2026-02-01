using BoltonCup.WebAPI.Dtos;
using BoltonCup.Core;
using BoltonCup.Infrastructure.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BoltonCup.WebAPI.Controllers;

public class InfoGuidesController(IInfoGuideRepository _infoGuides) : BoltonCupControllerBase
{
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<PaginatedList<InfoGuideDetailDto>>> GetInfoGuides([FromQuery] GetInfoGuidesQuery query)
    {
        return Ok(await _infoGuides.GetAllAsync<InfoGuideDetailDto>(query));
    }

    [AllowAnonymous]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<InfoGuideSingleDetailDto>> GetInfoGuideById(Guid id)
    {
        return OkOrNotFound(await _infoGuides.GetByIdAsync<InfoGuideSingleDetailDto>(id));
    }

    [AllowAnonymous]
    [HttpGet("tournament/{tournamentId:int}")]
    public async Task<ActionResult<InfoGuideSingleDetailDto>> GetInfoGuideByTournamentId(int tournamentId)
    {
        return OkOrNotFound(await _infoGuides.GetByTournamentIdAsync<InfoGuideSingleDetailDto>(tournamentId));
    }
    
}
