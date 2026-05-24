using BoltonCup.Core;
using BoltonCup.Core.BracketChallenge;
using Stripe;

namespace BoltonCup.WebAPI.Mapping;

/// <summary>Maps Stripe <see cref="PaymentIntent"/> objects to application commands.</summary>
public interface IStripeMapper
{
    /// <summary>Attempts to parse a tournament payment intent into a <see cref="ProcessTournamentPaymentIntentCommand"/>.</summary>
    bool TryParseTournamentPaymentCommand(PaymentIntent paymentIntent, out ProcessTournamentPaymentIntentCommand command);

    /// <summary>Attempts to parse a bracket challenge payment intent into a <see cref="ProcessBracketChallengePaymentIntentCommand"/>.</summary>
    bool TryParseBracketChallengePaymentCommand(PaymentIntent paymentIntent,
        out ProcessBracketChallengePaymentIntentCommand command);
}

/// <summary>Maps Stripe <see cref="PaymentIntent"/> objects to application commands.</summary>
public class StripeMapper() : IStripeMapper
{
    /// <inheritdoc/>
    public bool TryParseTournamentPaymentCommand(PaymentIntent paymentIntent, out ProcessTournamentPaymentIntentCommand command)
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

    /// <inheritdoc/>
    public bool TryParseBracketChallengePaymentCommand(PaymentIntent paymentIntent,
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