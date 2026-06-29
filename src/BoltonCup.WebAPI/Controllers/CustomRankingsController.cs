using BoltonCup.Core;
using BoltonCup.Shared;
using BoltonCup.WebAPI.Mapping;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static BoltonCup.Infrastructure.Identity.BoltonCupRole;
using static BoltonCup.WebAPI.Auth.BoltonCupPolicy;

namespace BoltonCup.WebAPI.Controllers;

/// <summary>Manages a user's private custom player rankings used to prepare for drafts.</summary>
[Authorize]
public class CustomRankingsController(
    ICustomRankingService _customRankingService,
    IAuthorizationService _authService,
    IMapper _mapper
) : BoltonCupControllerBase
{
    /// <summary>Gets the current user's custom rankings, optionally filtered by tournament.</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CustomRankingDto>>> GetCustomRankings([FromQuery] int? tournamentId = null)
    {
        var rankings = await _customRankingService.GetForAccountAsync(User.GetAccountId(), tournamentId);
        return Ok(_mapper.ToDtoList(rankings));
    }

    /// <summary>Gets custom rankings shared with the current user (view-only).</summary>
    [HttpGet("shared")]
    public async Task<ActionResult<IReadOnlyList<CustomRankingDto>>> GetSharedCustomRankings([FromQuery] int? tournamentId = null)
    {
        var rankings = await _customRankingService.GetSharedWithAccountAsync(User.GetAccountId(), tournamentId);
        return Ok(_mapper.ToDtoList(rankings));
    }

    /// <summary>Gets a single custom ranking by its ID (owner, admin, or a shared viewer).</summary>
    [HttpGet("{id:int}")]
    [Authorize(Policy = CanAccessRanking)]
    public async Task<ActionResult<CustomRankingSingleDto>> GetCustomRankingById(int id)
    {
        var ranking = await _customRankingService.GetByIdAsync(id);
        if (ranking is null)
            return NoContent();

        var canEdit = (await _authService.AuthorizeAsync(User, id, CanManageRanking)).Succeeded;

        // Auto-sync the pool on open for editors; read-only viewers see the ranking as-is.
        IReadOnlySet<int> stalePlayerIds = new HashSet<int>();
        if (canEdit)
        {
            stalePlayerIds = await _customRankingService.ReconcileAsync(id);
            ranking = await _customRankingService.GetByIdAsync(id);
        }

        return Ok(_mapper.ToDto(ranking, canEdit, stalePlayerIds));
    }

    /// <summary>Reconciles a ranking against the current tournament pool — auto-ranks new players (owner or admin only).</summary>
    [HttpPost("{id:int}/reconcile")]
    [Authorize(Policy = CanManageRanking)]
    public async Task<ActionResult<CustomRankingSingleDto>> ReconcileCustomRanking(int id)
    {
        var stalePlayerIds = await _customRankingService.ReconcileAsync(id);
        var ranking = await _customRankingService.GetByIdAsync(id);
        return Ok(_mapper.ToDto(ranking, canEdit: true, stalePlayerIds));
    }

    /// <summary>Removes a single player from a ranking — used to drop a stale entry (owner or admin only).</summary>
    [HttpDelete("{id:int}/players/{playerId:int}")]
    [Authorize(Policy = CanManageRanking)]
    public async Task<IActionResult> RemoveCustomRankingPlayer(int id, int playerId)
    {
        await _customRankingService.RemovePlayerAsync(id, playerId);
        return Ok();
    }

    /// <summary>Gets the accounts a ranking is shared with (owner or admin only).</summary>
    [HttpGet("{id:int}/shares")]
    [Authorize(Policy = CanManageRanking)]
    public async Task<ActionResult<IReadOnlyList<CustomRankingShareDto>>> GetCustomRankingShares(int id)
    {
        var shares = await _customRankingService.GetSharesAsync(id);
        return Ok(_mapper.ToShareDtoList(shares));
    }

    /// <summary>Shares a ranking (view-only) with another account — must be a GM of the tournament (owner or admin only).</summary>
    [HttpPost("{id:int}/shares")]
    [Authorize(Policy = CanManageRanking)]
    public async Task<IActionResult> ShareCustomRanking(int id, [FromBody] ShareCustomRankingRequest request)
    {
        await _customRankingService.AddShareAsync(id, request.AccountId);
        return Ok();
    }

    /// <summary>Removes a shared account from a ranking (owner or admin only).</summary>
    [HttpDelete("{id:int}/shares/{accountId:int}")]
    [Authorize(Policy = CanManageRanking)]
    public async Task<IActionResult> RemoveCustomRankingShare(int id, int accountId)
    {
        await _customRankingService.RemoveShareAsync(id, accountId);
        return Ok();
    }

    /// <summary>Searches accounts that can be invited to view a ranking — GMs of its tournament (owner or admin only).</summary>
    [HttpGet("{id:int}/invitable")]
    [Authorize(Policy = CanManageRanking)]
    public async Task<ActionResult<IReadOnlyList<RankingInviteUserDto>>> SearchInvitableAccounts(int id, [FromQuery] string? query = null)
    {
        var candidates = await _customRankingService.SearchInvitableGmsAsync(id, query);
        return Ok(_mapper.ToInviteDtoList(candidates));
    }

    /// <summary>Creates a new custom ranking (admin or tournament GM for their tournament).</summary>
    [HttpPost]
    public async Task<ActionResult<int>> CreateCustomRanking([FromBody] CreateCustomRankingRequest request)
    {
        if (!User.IsInRole(Admin) && !User.IsGmForTournament(request.TournamentId))
            return Forbid();

        var command = _mapper.ToCommand(request, User);
        var newId = await _customRankingService.CreateAsync(command);
        return Ok(newId);
    }

    /// <summary>Clones a ranking the user can access into a new ranking they own (must be a GM of its tournament).</summary>
    [HttpPost("{id:int}/clone")]
    [Authorize(Policy = CanAccessRanking)]
    public async Task<ActionResult<int>> CloneCustomRanking(int id, [FromBody] CloneCustomRankingRequest request)
    {
        var ranking = await _customRankingService.GetByIdAsync(id);
        if (ranking is null)
            return NotFound();

        if (!User.IsInRole(Admin) && !User.IsGmForTournament(ranking.TournamentId))
            return Forbid();

        var newId = await _customRankingService.CloneAsync(id, User.GetAccountId(), request.Title);
        return Ok(newId);
    }

    /// <summary>Updates a custom ranking's title and/or player order (owner or admin only).</summary>
    [HttpPut("{id:int}")]
    [Authorize(Policy = CanManageRanking)]
    public async Task<IActionResult> UpdateCustomRanking(int id, [FromBody] UpdateCustomRankingRequest request)
    {
        var command = _mapper.ToCommand(request);
        await _customRankingService.UpdateAsync(id, command);
        return Ok();
    }

    /// <summary>Deletes a custom ranking (owner or admin only).</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Policy = CanManageRanking)]
    public async Task<IActionResult> DeleteCustomRanking(int id)
    {
        await _customRankingService.DeleteAsync(id);
        return Ok();
    }
}
