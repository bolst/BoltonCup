using BoltonCup.Core;
using BoltonCup.WebAPI.Mapping;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using static BoltonCup.Infrastructure.Identity.BoltonCupRole;
using static BoltonCup.WebAPI.Auth.BoltonCupPolicy;
using static BoltonCup.Shared.HubEvents.Draft;

namespace BoltonCup.WebAPI.Controllers;

public class DraftsController(
    IDraftService _draftService,
    IDraftMapper _mapper,
    IAuthorizationService _authService
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
    
    [Authorize]
    [HttpPut("{id:int}/currentPick")]
    public async Task<IActionResult> DraftPlayer(
        int id, 
        [FromBody] DraftPlayerRequest request,
        [FromServices] IHubContext<Hubs.DraftHub> hubContext
    )
    {
        if (await _authService.AuthorizeAsync(User, request.TeamId, CanManageTeam) is {Succeeded: false})
            return Forbid();
        
        var command = _mapper.ToCommand(id, request);
        var draftState = await _draftService.DraftPlayerAsync(command);
        var payloadDto = _mapper.ToDto(draftState);
        await hubContext.Clients.Group($"Draft_{id}").SendAsync(OnPickMade, payloadDto);
        
        return Ok();
    }

    [Authorize]
    [HttpGet("{id:int}/ordering")]
    public async Task<ActionResult<IPagedList<DraftRankingDto>>> GetDraftRankings(int id, [FromQuery] GetDraftRankingsQuery query)
    {
        if (await _authService.AuthorizeAsync(User, id, CanAccessDraft) is {Succeeded: false})
            return Forbid();

        var rankings = await _draftService.GetDraftRankingsAsync(id, query);
        return Ok(_mapper.ToDtoList(rankings));
    }

    [Authorize(Roles = Admin)]
    [HttpPut("{id:int}/order")]
    public async Task<IActionResult> UpdateDraftOrder(int id, [FromBody] UpdateDraftOrderingRequest request)
    {
        var command = _mapper.ToCommand(id, request);
        await _draftService.UpdateOrderingAsync(command);
        return Ok();
    }
}