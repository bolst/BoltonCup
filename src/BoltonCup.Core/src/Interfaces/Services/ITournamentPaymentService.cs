using BoltonCup.Core.Commands;

namespace BoltonCup.Core;

public interface ITournamentPaymentService
{
    Task<TournamentPaymentIntent> CreateTournamentPaymentIntentAsync(CreateTournamentPaymentIntentCommand command, CancellationToken cancellationToken = default);
    Task ProcessPaymentIntentAsync(ProcessTournamentPaymentIntentCommand command, CancellationToken cancellationToken = default);
}

public record TournamentPaymentIntent(
    int AccountId,
    int TournamentId,
    decimal Amount,
    string Currency,
    string Secret,
    IReadOnlyList<PaymentBreakdown> AmountBreakdown
);

public record PaymentBreakdown(decimal Amount, string Title, string? Description = null);

public record ProcessTournamentPaymentIntentCommand(int AccountId, int TournamentId, string PaymentId);