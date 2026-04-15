using BoltonCup.Core.Queries.Base;

namespace BoltonCup.Core.BracketChallenge;

public interface IBracketChallengeService
{
    Task<IPagedList<Event>> GetBracketChallengesAsync(GetBracketChallengesQuery query, CancellationToken cancellationToken = default);
    Task UpdateLogoAsync(int eventId, string tempKey, CancellationToken cancellationToken = default);
    Task<BracketChallengePaymentIntent> CreatePaymentIntentAsync(CreateBracketChallengePaymentIntentCommand command, CancellationToken cancellationToken = default);
    Task ProcessPaymentIntentAsync(ProcessBracketChallengePaymentIntentCommand command, CancellationToken cancellationToken = default);
}

public sealed record GetBracketChallengesQuery : QueryBase;

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