using BoltonCup.Core;
using BoltonCup.Infrastructure.Extensions;
using BoltonCup.WebAPI.Mapping;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static BoltonCup.WebAPI.Authentication.BoltonCupPolicy;

namespace BoltonCup.WebAPI.Controllers;

[Route("api/tournaments/{id:int}/payments")]
public class TournamentPaymentsController(ITournamentPaymentService _paymentService, ITournamentPaymentMapper _paymentMapper) 
    : BoltonCupControllerBase
{
    [Authorize(Policy = RequireCompletedAccount)]
    [HttpPost("createIntent")]
    public async Task<ActionResult<TournamentPaymentIntentDto>> CreateTournamentPaymentIntent(int id, [FromBody] CreateTournamentPaymentIntentRequest request)
    {
        var accountId = User.GetAccountId();
        var command = _paymentMapper.ToCommand(id, accountId, request);
        var result = await _paymentService.CreateTournamentPaymentIntentAsync(command);
        return _paymentMapper.ToDto(result);
    }
}