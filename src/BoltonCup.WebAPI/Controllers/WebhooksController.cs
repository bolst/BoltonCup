using BoltonCup.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace BoltonCup.WebAPI.Controllers;

public class WebhooksController(ITournamentPaymentService _paymentService, ILogger<WebhooksController> _logger) 
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
            _logger.LogError(e, "Error on stripe webhook");
            return BadRequest();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Internal error on stripe webhook");
            return StatusCode(500);
        }
    }
}