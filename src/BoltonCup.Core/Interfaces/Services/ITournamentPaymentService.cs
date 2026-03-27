using BoltonCup.Core.Commands;

namespace BoltonCup.Core;

public interface ITournamentPaymentService
{
    Task<TournamentPaymentIntent> CreateTournamentPaymentIntentAsync(CreateTournamentPaymentIntentCommand command, CancellationToken cancellationToken = default);
    Task ProcessPaymentIntentAsync(string data, string signature, CancellationToken cancellationToken = default);
}

public record TournamentPaymentIntent(
    int AccountId,
    int TournamentId,
    decimal Amount,
    string Secret
);