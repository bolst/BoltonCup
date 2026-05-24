using BoltonCup.Core;
using BoltonCup.Infrastructure.Extensions;
using BoltonCup.Shared;
using BoltonCup.WebAPI.Mapping;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static BoltonCup.WebAPI.Auth.BoltonCupPolicy;

namespace BoltonCup.WebAPI.Controllers;

/// <summary>Manages Stripe payment intent creation for tournament registrations.</summary>
[Route("api/tournaments/{id:int}/payments")]
public class TournamentPaymentsController(ITournamentPaymentService _paymentService, IMapper _mapper)
    : BoltonCupControllerBase
{
    /// <summary>Creates a Stripe payment intent for a tournament registration.</summary>
    [Authorize(Policy = RequireCompletedAccount)]
    [HttpPost("createIntent")]
    public async Task<ActionResult<TournamentPaymentIntentDto>> CreateTournamentPaymentIntent(int id, [FromBody] CreateTournamentPaymentIntentRequest request)
    {
        var accountId = User.GetAccountId();
        var command = _mapper.ToCommand(id, accountId, request);
        var result = await _paymentService.CreateTournamentPaymentIntentAsync(command);
        return _mapper.ToDto(result);
    }
}