using BoltonCup.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace BoltonCup.WebAPI.Controllers;

public class WebhooksController(ITournamentPaymentService _paymentService) 
    : BoltonCupControllerBase
{
    [AllowAnonymous]
    [HttpPost("stripe")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> StripeWebhook()
    {
        try
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            await _paymentService.ProcessPaymentIntentAsync(json, Request.Headers["Stripe-Signature"].ToString());
            return Ok();
        }
        catch (StripeException e)
        {
            return BadRequest();
        }
        catch (Exception e)
        {
            return StatusCode(500);
        }
    }
}