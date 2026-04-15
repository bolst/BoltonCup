using BoltonCup.Core;
using BoltonCup.Core.BracketChallenge;
using BoltonCup.Infrastructure;
using BoltonCup.Infrastructure.Settings;
using BoltonCup.WebAPI.Mapping;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Stripe;

namespace BoltonCup.WebAPI.Controllers;

public class WebhooksController(
    IOptions<StripeSettings> _stripeSettings, 
    IStripeMapper _mapper,
    ITournamentPaymentService _tournamentPaymentService,
    IBracketChallengeService _bracketChallengeService,
    ILogger<WebhooksController> _logger
) : BoltonCupControllerBase
{
    
    private readonly string _stripeWebhookSecret = _stripeSettings.Value.WebhookSecret;
    
    [AllowAnonymous]
    [HttpPost("stripe")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> StripeWebhook()
    {
        try
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            var signature = Request.Headers["Stripe-Signature"].ToString();
            var stripeEvent = EventUtility.ConstructEvent(json, signature, _stripeWebhookSecret);

            if (stripeEvent.Type == EventTypes.PaymentIntentSucceeded 
                && stripeEvent.Data.Object is PaymentIntent paymentIntent)
            {
                if (!paymentIntent.Metadata.TryGetValue(nameof(PurchaseType), out var purchaseType))
                {
                    _logger.LogWarning("No purchase type metadata in stripe webhook");
                    return Ok();
                }
                
                switch (purchaseType)
                {
                    case PurchaseType.TournamentRegistration:
                        if (_mapper.TryParseTournamentPaymentCommand(paymentIntent, out var tournamentCommand))
                            await _tournamentPaymentService.ProcessPaymentIntentAsync(tournamentCommand);
                        break;
                    case PurchaseType.BracketChallengeRegistration:
                        if (_mapper.TryParseBracketChallengePaymentCommand(paymentIntent, out var bracketChallengeCommand))
                            await _bracketChallengeService.ProcessPaymentIntentAsync(bracketChallengeCommand);
                        break;
                    default:
                        _logger.LogWarning("Unhandled purchase type {PurchaseType}", purchaseType);
                        break;
                }
            }

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