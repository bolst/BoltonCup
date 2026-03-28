using BoltonCup.Core;

namespace BoltonCup.WebAPI.Mapping;

public record TournamentPaymentIntentDto(
    string ClientSecret,
    decimal TotalAmount,
    IReadOnlyList<PaymentBreakdown> Breakdown
);
