using BoltonCup.Core.BracketChallenge;
using BoltonCup.WebAPI.Mapping;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BoltonCup.WebAPI.Controllers;

public class BracketChallengesController(
    IBracketChallengeService _bracketChallengeService, 
    IBracketChallengeMapper _mapper
) : BoltonCupControllerBase
{
    [AllowAnonymous]
    [HttpPost("{id:int}")]
    public async Task<ActionResult<BracketChallengePaymentIntentDto>> CreateBracketChallengePaymentIntent(int id, [FromBody] CreateBracketChallengePaymentIntentRequest request)
    {
        var command = _mapper.ToCommand(id, request);
        var result = await _bracketChallengeService.CreatePaymentIntentAsync(command);
        return _mapper.ToDto(result);
    }
}