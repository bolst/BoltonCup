using BoltonCup.Core;

namespace BoltonCup.WebAPI.Mapping;

public record BracketChallengePaymentIntentDto(
    string ClientSecret,
    decimal TotalAmount,
    string Currency,
    IReadOnlyList<PaymentBreakdown> Breakdown
);
