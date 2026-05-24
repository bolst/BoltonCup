using BoltonCup.Core;
using BoltonCup.WebAPI.Mapping;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using static BoltonCup.Infrastructure.Identity.BoltonCupRole;
using static BoltonCup.WebAPI.Auth.BoltonCupPolicy;
using static BoltonCup.Shared.HubEvents.Draft;

namespace BoltonCup.WebAPI.Controllers;

/// <summary>Manages draft creation, state transitions, pick submissions, and player rankings.</summary>
public class DraftsController(
    IDraftService _draftService,
    IDraftMapper _mapper,
    IAuthorizationService _authService
) : BoltonCupControllerBase
{
    /// <summary>Gets a paginated list of drafts.</summary>
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<IPagedList<DraftDto>>> GetDrafts([FromQuery] GetDraftsRequest request)
    {
        var query = _mapper.ToQuery(request);
        var result = await _draftService.GetAsync(query);
        return Ok(_mapper.ToDtoList(result));
    }

    /// <summary>Gets a single draft by its ID.</summary>
    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<DraftSingleDto>> GetDraftById(int id)
    {
        var result = await _draftService.GetByIdAsync(id);
        var authorized = await _authService.AuthorizeAsync(User, id, CanAccessDraft) is { Succeeded: true };
        return OkOrNoContent(_mapper.ToDto(result, authorized));
    }

    /// <summary>Creates a new draft (admin only).</summary>
    [Authorize(Roles = Admin)]
    [HttpPost]
    public async Task<ActionResult<int>> CreateDraft([FromBody] CreateDraftRequest request)
    {
        var command = _mapper.ToCommand(request);
        var newDraftId = await _draftService.CreateAsync(command);
        return Ok(newDraftId);
    }

    /// <summary>Updates draft settings and broadcasts the new state to connected clients (admin only).</summary>
    [Authorize(Roles = Admin)]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateDraft(
        int id,
        [FromBody] UpdateDraftRequest request,
        [FromServices] IHubContext<Hubs.DraftHub> hubContext
    )
    {
        var command = _mapper.ToCommand(request);
        var draftState = await _draftService.UpdateAsync(id, command);
        
        var authorized = await _authService.AuthorizeAsync(User, id, CanAccessDraft) is { Succeeded: true };
        var payloadDto = _mapper.ToDto(draftState, authorized);
        await hubContext.Clients.Group($"Draft_{id}").SendAsync(OnDraftUpdate, payloadDto);
        
        return Ok();
    }

    /// <summary>Starts the draft and notifies connected clients (admin only).</summary>
    [Authorize(Roles = Admin)]
    [HttpPatch("{id:int}/start")]
    public async Task<IActionResult> StartDraft(int id, [FromServices] IHubContext<Hubs.DraftHub> hubContext)
    {
        await _draftService.StartAsync(id);
        await hubContext.Clients.Group($"Draft_{id}").SendAsync(OnDraftStatusChange, DraftStatus.InProgress);
        return Ok();
    }
    
    /// <summary>Pauses the draft and notifies connected clients (admin only).</summary>
    [Authorize(Roles = Admin)]
    [HttpPatch("{id:int}/pause")]
    public async Task<IActionResult> PauseDraft(int id, [FromServices] IHubContext<Hubs.DraftHub> hubContext)
    {
        await _draftService.PauseAsync(id);
        await hubContext.Clients.Group($"Draft_{id}").SendAsync(OnDraftStatusChange, DraftStatus.Paused);
        return Ok();
    }
    
    /// <summary>Ends the draft and notifies connected clients (admin only).</summary>
    [Authorize(Roles = Admin)]
    [HttpPatch("{id:int}/end")]
    public async Task<IActionResult> EndDraft(int id, [FromServices] IHubContext<Hubs.DraftHub> hubContext)
    {
        await _draftService.EndAsync(id);
        await hubContext.Clients.Group($"Draft_{id}").SendAsync(OnDraftStatusChange, DraftStatus.Completed);
        return Ok();
    }
    
    /// <summary>Deletes a draft (admin only).</summary>
    [Authorize(Roles = Admin)]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteDraft(int id)
    {
        await _draftService.DeleteAsync(id);
        return Ok();
    }

    /// <summary>Gets the current pick for the specified draft.</summary>
    [AllowAnonymous]
    [HttpGet("{id:int}/currentPick")]
    public async Task<ActionResult<DraftPickSingleDto>> GetCurrentDraftPick(int id)
    {
        var result = await _draftService.GetCurrentPickAsync(id);
        return OkOrNoContent(_mapper.ToDto(result));
    }
    
    /// <summary>Submits a player pick for the current draft slot.</summary>
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

    /// <summary>Gets the player rankings for a draft.</summary>
    [AllowAnonymous]
    [HttpGet("{id:int}/players")]
    public async Task<ActionResult<IPagedList<DraftRankingDto>>> GetDraftPlayerRankings(int id, [FromQuery] GetDraftRankingsQuery query)
    {
        var rankings = await _draftService.GetDraftRankingsAsync(id, query);
        return Ok(_mapper.ToDtoList(rankings));
    }
}