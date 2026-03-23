using BoltonCup.Core;
using BoltonCup.WebAPI.Mapping;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BoltonCup.WebAPI.Controllers;

public class InfoGuidesController(IInfoGuideRepository _infoGuides, IInfoGuideMapper _mapper) : BoltonCupControllerBase
{
    /// <remarks>
    /// Gets a paginated list of info guides.
    /// </remarks>
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<IPagedList<InfoGuideDto>>> GetInfoGuides([FromQuery] GetInfoGuidesQuery query)
    {
        var guides = await _infoGuides.GetAllAsync(query);
        return Ok(_mapper.ToDtoList(guides));
    }

    /// <remarks>
    /// Gets a single info guide by its ID.
    /// </remarks>
    [AllowAnonymous]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<InfoGuideSingleDto>> GetInfoGuideById(Guid id)
    {
        var guide = await _infoGuides.GetByIdAsync(id);
        return OkOrNoContent(_mapper.ToDto(guide));
    }

    /// <remarks>
    /// Gets an info guide by tournament ID.
    /// </remarks>
    [AllowAnonymous]
    [HttpGet("tournament/{tournamentId:int}")]
    public async Task<ActionResult<InfoGuideSingleDto>> GetInfoGuideByTournamentId(int tournamentId)
    {
        var guide = await _infoGuides.GetByTournamentIdAsync(tournamentId);
        return OkOrNoContent(_mapper.ToDto(guide));
    }
    
}
