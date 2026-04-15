using BoltonCup.Core;
using BoltonCup.Core.BracketChallenge;
using Stripe;

namespace BoltonCup.WebAPI.Mapping;

public interface IStripeMapper
{
    bool TryParseTournamentPaymentCommand(PaymentIntent paymentIntent, out ProcessTournamentPaymentIntentCommand command);

    bool TryParseBracketChallengePaymentCommand(PaymentIntent paymentIntent,
        out ProcessBracketChallengePaymentIntentCommand command);
}

public class StripeMapper() : IStripeMapper
{
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

    public bool TryParseBracketChallengePaymentCommand(PaymentIntent paymentIntent,
        out ProcessBracketChallengePaymentIntentCommand command)
    {
        command = null!;
        if (paymentIntent.Metadata.TryGetValue("EventId", out var eventIdStr)
            && paymentIntent.Metadata.TryGetValue("Name", out var name)
            && paymentIntent.Metadata.TryGetValue("Email", out var email)
            && int.TryParse(eventIdStr, out var eventId))
        {
            command = new ProcessBracketChallengePaymentIntentCommand(
                EventId: eventId,
                Name: name, 
                Email: email, 
                PaymentId: paymentIntent.Id
            );
            return true;
        }

        return false;
    }
}