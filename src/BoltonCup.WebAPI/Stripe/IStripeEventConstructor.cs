using Stripe;

namespace BoltonCup.WebAPI.Stripe;

/// <summary>Constructs and validates Stripe webhook events from raw request data.</summary>
public interface IStripeEventConstructor
{
    /// <summary>Constructs a Stripe <see cref="Event"/> by validating the webhook signature.</summary>
    Event ConstructEvent(string json, string signature, string secret);
}

/// <summary>Constructs and validates Stripe webhook events from raw request data.</summary>
public class StripeEventConstructor : IStripeEventConstructor
{
    /// <inheritdoc/>
    public Event ConstructEvent(string json, string signature, string secret)
        => EventUtility.ConstructEvent(json, signature, secret);
}
