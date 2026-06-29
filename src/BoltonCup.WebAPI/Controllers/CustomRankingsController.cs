using BoltonCup.Core;
using BoltonCup.Shared;
using BoltonCup.WebAPI.Mapping;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static BoltonCup.Infrastructure.Identity.BoltonCupRole;

namespace BoltonCup.WebAPI.Controllers;

/// <summary>Manages a user's private custom player rankings used to prepare for drafts.</summary>
[Authorize]
public class CustomRankingsController(
    ICustomRankingService _customRankingService,
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
    public async Task<ActionResult<CustomRankingSingleDto>> GetCustomRankingById(int id)
    {
        var ranking = await _customRankingService.GetByIdAsync(id);
        if (ranking is null)
            return NoContent();

        var accountId = User.GetAccountId();
        var canEdit = ranking.AccountId == accountId || User.IsInRole(Admin);
        var isSharedWith = ranking.SharedWith.Any(s => s.SharedWithAccountId == accountId);
        if (!canEdit && !isSharedWith)
            return Forbid();

        return Ok(_mapper.ToDto(ranking, canEdit));
    }

    /// <summary>Gets the accounts a ranking is shared with (owner or admin only).</summary>
    [HttpGet("{id:int}/shares")]
    public async Task<ActionResult<IReadOnlyList<CustomRankingShareDto>>> GetCustomRankingShares(int id)
    {
        var ranking = await _customRankingService.GetByIdAsync(id);
        if (ranking is null)
            return NotFound();
        if (ranking.AccountId != User.GetAccountId() && !User.IsInRole(Admin))
            return Forbid();

        var shares = await _customRankingService.GetSharesAsync(id);
        return Ok(_mapper.ToShareDtoList(shares));
    }

    /// <summary>Shares a ranking (view-only) with another account — must be a GM of the tournament (owner or admin only).</summary>
    [HttpPost("{id:int}/shares")]
    public async Task<IActionResult> ShareCustomRanking(int id, [FromBody] ShareCustomRankingRequest request)
    {
        var ranking = await _customRankingService.GetByIdAsync(id);
        if (ranking is null)
            return NotFound();
        if (ranking.AccountId != User.GetAccountId() && !User.IsInRole(Admin))
            return Forbid();

        await _customRankingService.AddShareAsync(id, request.AccountId);
        return Ok();
    }

    /// <summary>Removes a shared account from a ranking (owner or admin only).</summary>
    [HttpDelete("{id:int}/shares/{accountId:int}")]
    public async Task<IActionResult> RemoveCustomRankingShare(int id, int accountId)
    {
        var ranking = await _customRankingService.GetByIdAsync(id);
        if (ranking is null)
            return NotFound();
        if (ranking.AccountId != User.GetAccountId() && !User.IsInRole(Admin))
            return Forbid();

        await _customRankingService.RemoveShareAsync(id, accountId);
        return Ok();
    }

    /// <summary>Searches accounts that can be invited to view a ranking — GMs of its tournament (owner or admin only).</summary>
    [HttpGet("{id:int}/invitable")]
    public async Task<ActionResult<IReadOnlyList<RankingInviteUserDto>>> SearchInvitableAccounts(int id, [FromQuery] string? query = null)
    {
        var ranking = await _customRankingService.GetByIdAsync(id);
        if (ranking is null)
            return NotFound();
        if (ranking.AccountId != User.GetAccountId() && !User.IsInRole(Admin))
            return Forbid();

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

    /// <summary>Updates a custom ranking's title and/or player order (owner or admin only).</summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateCustomRanking(int id, [FromBody] UpdateCustomRankingRequest request)
    {
        var ranking = await _customRankingService.GetByIdAsync(id);
        if (ranking is null)
            return NotFound();
        if (ranking.AccountId != User.GetAccountId() && !User.IsInRole(Admin))
            return Forbid();

        var command = _mapper.ToCommand(request);
        await _customRankingService.UpdateAsync(id, command);
        return Ok();
    }

    /// <summary>Deletes a custom ranking (owner or admin only).</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteCustomRanking(int id)
    {
        var ranking = await _customRankingService.GetByIdAsync(id);
        if (ranking is null)
            return NotFound();
        if (ranking.AccountId != User.GetAccountId() && !User.IsInRole(Admin))
            return Forbid();

        await _customRankingService.DeleteAsync(id);
        return Ok();
    }
}
