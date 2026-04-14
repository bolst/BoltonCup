using BoltonCup.Core.BracketChallenge;
using BoltonCup.WebAPI.Mapping;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BoltonCup.WebAPI.Controllers;

public class BracketChallengesController(
    IBracketChallengeService _bracketChallengeService, 
    IBracketChallengeMapper _mapper, 
    ILogger<BracketChallengesController> _logger
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
    
    [AllowAnonymous]
    [HttpPost("stripe")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> StripeWebhook()
    {
        try
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            await _bracketChallengeService.ProcessPaymentIntentAsync(json, Request.Headers["Stripe-Signature"].ToString());
            return Ok();
        }
        catch (Stripe.StripeException e)
        {
            _logger.LogError(e, "Error on bracket challenge stripe webhook");
            return BadRequest();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Internal error on bracket challenge stripe webhook");
            return StatusCode(500);
        }
    }
}