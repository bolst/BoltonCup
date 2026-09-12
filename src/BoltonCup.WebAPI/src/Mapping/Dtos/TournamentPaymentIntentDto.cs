using BoltonCup.Core;

namespace BoltonCup.WebAPI.Mapping;

/// <summary>DTO representing a payment intent for a tournament registration fee.</summary>
/// <param name="ClientSecret">The Stripe client secret for the payment intent.</param>
/// <param name="TotalAmount">The total amount to be charged.</param>
/// <param name="Currency">The currency code for the payment.</param>
/// <param name="Breakdown">The itemized breakdown of the payment amount.</param>
public record TournamentPaymentIntentDto(
    string ClientSecret,
    decimal TotalAmount,
    string Currency,
    IReadOnlyList<PaymentBreakdown> Breakdown
);