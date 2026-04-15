namespace BoltonCup.Infrastructure;

public static class FeeCalculator
{
    /// <summary>
    /// Returns the input amount adjusted for Stripe (e.g., $150 -> $154.79).
    /// </summary>
    /// <remarks>
    /// Stripe charges 2.9% + 0.3c.
    /// </remarks>
    public static decimal GetAdjustedStripeAmount(decimal amount)
    {
        return (amount + (decimal)0.3) / (decimal)(1 - 0.029);
    }
}