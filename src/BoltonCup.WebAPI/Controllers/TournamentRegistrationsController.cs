using BoltonCup.Core;
using BoltonCup.Infrastructure.Extensions;
using BoltonCup.Shared;
using static BoltonCup.WebAPI.Auth.BoltonCupPolicy;
using BoltonCup.WebAPI.Mapping;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BoltonCup.WebAPI.Controllers;

/// <summary>Manages the authenticated user's tournament registration.</summary>
[Route("api/tournaments/{id:int}/registrations")]
public class TournamentRegistrationsController(
    ITournamentRegistrationService _registrationService,
    ITournamentRegistrationMapper _registrationMapper
) : BoltonCupControllerBase
{
    /// <summary>Gets the authenticated user's registration for a tournament.</summary>
    [Authorize(Policy = RequireCompletedAccount)]
    [HttpGet]
    public async Task<ActionResult<TournamentRegistrationDto>> GetMyTournamentRegistration(int id)
    {
        var accountId = User.GetAccountId();
        var tournament = await _registrationService.GetAsync(id, accountId);
        return OkOrNoContent(_registrationMapper.ToDto(tournament));
    }

    /// <summary>Creates or updates the authenticated user's registration for a tournament.</summary>
    [Authorize(Policy = RequireCompletedAccount)]
    [HttpPost]
    public async Task<IActionResult> UpdateMyTournamentRegistration(int id, [FromBody] TournamentRegistrationDto data)
    {
        var accountId = User.GetAccountId();
        var command = new UpsertTournamentRegistrationCommand(id, accountId, data.CurrentStep, data.IsComplete, data.Payload);
        await _registrationService.UpsertAsync(command);
        return Ok();
    }
}