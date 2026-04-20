using BoltonCup.Core;
using BoltonCup.WebAPI.Mapping;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static BoltonCup.Infrastructure.Identity.BoltonCupRole;

namespace BoltonCup.WebAPI.Controllers;

public class DraftsController(
    IDraftService _draftService,
    IDraftMapper _mapper
) : BoltonCupControllerBase
{
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<IPagedList<DraftDto>>> GetDrafts([FromQuery] GetDraftsRequest request)
    {
        var query = _mapper.ToQuery(request);
        var result = await _draftService.GetAsync(query);
        return Ok(_mapper.ToDtoList(result));
    }

    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<DraftSingleDto>> GetDraftById(int id)
    {
        var result = await _draftService.GetByIdAsync(id);
        return OkOrNoContent(_mapper.ToDto(result));
    }

    [Authorize(Roles = Admin)]
    [HttpPost]
    public async Task<ActionResult<int>> CreateDraft([FromBody] CreateDraftRequest request)
    {
        var command = _mapper.ToCommand(request);
        var newDraftId = await _draftService.CreateAsync(command);
        return Ok(newDraftId);
    }

    [Authorize(Roles = Admin)]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateDraft(int id, [FromBody] UpdateDraftRequest request)
    {
        var command = _mapper.ToCommand(id, request);
        await _draftService.UpdateAsync(command);
        return Ok();
    }
    
    [Authorize(Roles = Admin)]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteDraft(int id)
    {
        await _draftService.DeleteAsync(id);
        return Ok();
    }
    
    [AllowAnonymous]
    [HttpGet("{id:int}/currentPick")]
    public async Task<ActionResult<DraftPickSingleDto>> GetCurrentDraftPick(int id)
    {
        var result = await _draftService.GetCurrentPickAsync(id);
        return OkOrNoContent(_mapper.ToDto(result));
    }

    [Authorize(Roles = Admin)]
    [HttpPut("{id:int}/currentPick")]
    public async Task<IActionResult> DraftPlayer(int id, [FromBody] DraftPlayerRequest request)
    {
        var command = _mapper.ToCommand(id, request);
        await _draftService.DraftPlayerAsync(command);
        return Ok();
    }
}