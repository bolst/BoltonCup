using BoltonCup.Integrations.Payments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BoltonCup.WebAPI.Controllers;

/// <summary>Handles incoming webhook events from third-party services.</summary>
public class WebhooksController(
    IStripeWebhookProcessor _webhookProcessor,
    ILogger<WebhooksController> _logger
) : BoltonCupControllerBase
{
    /// <summary>Receives and processes Stripe webhook events.</summary>
    [AllowAnonymous]
    [HttpPost("stripe")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> StripeWebhook()
    {
        try
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            var signature = Request.Headers["Stripe-Signature"].ToString();
            await _webhookProcessor.ProcessAsync(json, signature);

            return Ok();
        }
        catch (StripeWebhookVerificationException e)
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
