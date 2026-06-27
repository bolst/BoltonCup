using BoltonCup.Core;
using BoltonCup.Shared;
using BoltonCup.WebAPI.Mapping;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static BoltonCup.Infrastructure.Identity.BoltonCupRole;
using static BoltonCup.WebAPI.Auth.BoltonCupPolicy;

namespace BoltonCup.WebAPI.Controllers;

/// <summary>Manages player trades between teams: proposal, acceptance, decline, cancellation, and admin approval.</summary>
public class TradesController(
    ITradeService _tradeService,
    IDraftService _draftService,
    IMapper _mapper
) : BoltonCupControllerBase
{
    /// <summary>Gets all trades for a tournament. Accessible to the tournament's GMs and admins.</summary>
    [Authorize(Policy = RequireCompletedAccount)]
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TradeDto>>> GetTrades([FromQuery] int tournamentId)
    {
        if (!User.IsInRole(Admin) && !User.IsGmForTournament(tournamentId))
        {
            return Forbid();
        }

        var trades = await _tradeService.GetByTournamentAsync(tournamentId);
        var viewer = new TradeViewerContext(User.GetAccountIdOrDefault(), User.IsInRole(Admin));
        return Ok(_mapper.ToDtoList(trades, viewer));
    }

    /// <summary>Gets per-player game availability for a tournament, used to inform trade decisions. Accessible to the tournament's GMs and admins.</summary>
    [Authorize(Policy = RequireCompletedAccount)]
    [HttpGet("player-availability")]
    public async Task<ActionResult<IReadOnlyList<PlayerAvailabilityDto>>> GetPlayerAvailability([FromQuery] int tournamentId)
    {
        if (!User.IsInRole(Admin) && !User.IsGmForTournament(tournamentId))
        {
            return Forbid();
        }

        var availability = await _draftService.GetTournamentAvailabilityAsync(tournamentId);
        return Ok(_mapper.ToPlayerAvailabilityList(availability));
    }

    /// <summary>Proposes a trade. The caller must be the proposing team's GM (or an admin).</summary>
    [Authorize(Policy = RequireCompletedAccount)]
    [HttpPost]
    public async Task<ActionResult<int>> CreateTrade([FromBody] CreateTradeRequest request)
    {
        if (!User.IsInRole(Admin) && !User.IsGmForTeam(request.ProposingTeamId))
        {
            return Forbid();
        }

        var command = _mapper.ToCommand(request, User);
        var id = await _tradeService.CreateAsync(command);
        return Ok(id);
    }

    /// <summary>Accepts a pending trade. The caller must be the receiving team's GM.</summary>
    [Authorize(Policy = RequireCompletedAccount)]
    [HttpPost("{id:int}/accept")]
    public async Task<IActionResult> AcceptTrade(int id)
    {
        var trade = await _tradeService.GetByIdAsync(id);
        if (trade is null)
        {
            return NotFound();
        }
        if (!User.IsGmForTeam(trade.ReceivingTeamId))
        {
            return Forbid();
        }

        await _tradeService.AcceptAsync(id, User.GetAccountId());
        return Ok();
    }

    /// <summary>Declines a pending trade. The caller must be the receiving team's GM.</summary>
    [Authorize(Policy = RequireCompletedAccount)]
    [HttpPost("{id:int}/decline")]
    public async Task<IActionResult> DeclineTrade(int id)
    {
        var trade = await _tradeService.GetByIdAsync(id);
        if (trade is null)
        {
            return NotFound();
        }
        if (!User.IsGmForTeam(trade.ReceivingTeamId))
        {
            return Forbid();
        }

        await _tradeService.DeclineAsync(id, User.GetAccountId());
        return Ok();
    }

    /// <summary>Cancels a trade. The proposing team's GM may cancel while pending; an admin may cancel while pending or accepted.</summary>
    [Authorize(Policy = RequireCompletedAccount)]
    [HttpPost("{id:int}/cancel")]
    public async Task<IActionResult> CancelTrade(int id)
    {
        var trade = await _tradeService.GetByIdAsync(id);
        if (trade is null)
        {
            return NotFound();
        }

        var isAdmin = User.IsInRole(Admin);
        if (!isAdmin && !User.IsGmForTeam(trade.ProposingTeamId))
        {
            return Forbid();
        }

        await _tradeService.CancelAsync(id, User.GetAccountId(), isAdmin);
        return Ok();
    }

    /// <summary>Approves an accepted trade and processes the roster changes (admin only).</summary>
    [Authorize(Roles = Admin)]
    [HttpPost("{id:int}/approve")]
    public async Task<IActionResult> ApproveTrade(int id)
    {
        await _tradeService.ApproveAsync(id, User.GetAccountId());
        return Ok();
    }
}
