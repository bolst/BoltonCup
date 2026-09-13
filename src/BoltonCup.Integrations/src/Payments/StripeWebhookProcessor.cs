using BoltonCup.Core;
using BoltonCup.Core.BracketChallenge;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stripe;
using StripeEvent = Stripe.Event;

namespace BoltonCup.Integrations.Payments;

/// <summary>Thrown when an incoming Stripe webhook payload fails signature verification.</summary>
public class StripeWebhookVerificationException(string message, Exception innerException) : Exception(message, innerException);

public interface IStripeWebhookProcessor
{
    /// <summary>Verifies and processes a raw Stripe webhook payload.</summary>
    /// <exception cref="StripeWebhookVerificationException">The payload's signature could not be verified.</exception>
    Task ProcessAsync(string json, string signature, CancellationToken cancellationToken = default);
}

public class StripeWebhookProcessor(
    IOptions<StripeSettings> _stripeSettings,
    IStripeEventConstructor _eventConstructor,
    ITournamentPaymentService _tournamentPaymentService,
    IBracketChallengeService _bracketChallengeService,
    ILogger<StripeWebhookProcessor> _logger
) : IStripeWebhookProcessor
{
    public async Task ProcessAsync(string json, string signature, CancellationToken cancellationToken = default)
    {
        StripeEvent stripeEvent;
        try
        {
            stripeEvent = _eventConstructor.ConstructEvent(json, signature, _stripeSettings.Value.WebhookSecret);
        }
        catch (StripeException e)
        {
            throw new StripeWebhookVerificationException("Failed to verify stripe webhook signature.", e);
        }

        if (stripeEvent.Type != EventTypes.PaymentIntentSucceeded || stripeEvent.Data.Object is not PaymentIntent paymentIntent)
        {
            return;
        }

        if (!paymentIntent.Metadata.TryGetValue(nameof(PurchaseType), out var purchaseType))
        {
            _logger.LogWarning("No purchase type metadata in stripe webhook");
            return;
        }

        switch (purchaseType)
        {
            case PurchaseType.TournamentRegistration:
                if (TryParseTournamentPaymentCommand(paymentIntent, out var tournamentCommand))
                {
                    await _tournamentPaymentService.ProcessPaymentIntentAsync(tournamentCommand, cancellationToken);
                }

                break;
            case PurchaseType.BracketChallengeRegistration:
                if (TryParseBracketChallengePaymentCommand(paymentIntent, out var bracketChallengeCommand))
                {
                    await _bracketChallengeService.ProcessPaymentIntentAsync(bracketChallengeCommand, cancellationToken);
                }

                break;
            default:
                _logger.LogWarning("Unhandled purchase type {PurchaseType}", purchaseType);
                break;
        }
    }

    public static bool TryParseTournamentPaymentCommand(PaymentIntent paymentIntent, out ProcessTournamentPaymentIntentCommand command)
    {
        command = null!;
        if (paymentIntent.Metadata.TryGetValue("AccountId", out var accountIdStr)
            && paymentIntent.Metadata.TryGetValue("TournamentId", out var tournamentIdStr)
            && int.TryParse(accountIdStr, out var accountId)
            && int.TryParse(tournamentIdStr, out var tournamentId))
        {
            command = new ProcessTournamentPaymentIntentCommand(
                AccountId: accountId,
                TournamentId: tournamentId,
                PaymentId: paymentIntent.Id
            );
            return true;
        }

        return false;
    }

    public static bool TryParseBracketChallengePaymentCommand(
        PaymentIntent paymentIntent,
        out ProcessBracketChallengePaymentIntentCommand command)
    {
        command = null!;
        if (paymentIntent.Metadata.TryGetValue("EventId", out var eventIdStr)
            && paymentIntent.Metadata.TryGetValue("Name", out var name)
            && paymentIntent.Metadata.TryGetValue("Email", out var email)
            && paymentIntent.Metadata.TryGetValue("AgreedToTOS", out var agreedToTOSStr)
            && int.TryParse(eventIdStr, out var eventId))
        {
            command = new ProcessBracketChallengePaymentIntentCommand(
                EventId: eventId,
                Name: name,
                Email: email,
                PaymentId: paymentIntent.Id,
                AgreedToTOS: agreedToTOSStr == "true"
            );
            return true;
        }

        return false;
    }
}
