namespace BoltonCup.Core.BracketChallenge;

public interface IBracketChallengeService
{
    Task<BracketChallengePaymentIntent> CreatePaymentIntentAsync(CreateBracketChallengePaymentIntentCommand command, CancellationToken cancellationToken = default);
    Task ProcessPaymentIntentAsync(ProcessBracketChallengePaymentIntentCommand command, CancellationToken cancellationToken = default);
}

public sealed record BracketChallengePaymentIntent(
    int EventId,
    string Name,
    string Email,
    decimal Amount,
    string Currency,
    string Secret,
    IReadOnlyList<PaymentBreakdown> AmountBreakdown
);

public sealed record CreateBracketChallengePaymentIntentCommand(string Name, string Email, int BracketChallengeId);

public sealed record ProcessBracketChallengePaymentIntentCommand(int EventId, string Name, string Email, string PaymentId);