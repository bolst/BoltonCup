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

    /// <summary>Gets a single custom ranking by its ID (owner or admin only).</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<CustomRankingSingleDto>> GetCustomRankingById(int id)
    {
        var ranking = await _customRankingService.GetByIdAsync(id);
        if (ranking is null)
            return NoContent();
        if (ranking.AccountId != User.GetAccountId() && !User.IsInRole(Admin))
            return Forbid();
        return Ok(_mapper.ToDto(ranking));
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
