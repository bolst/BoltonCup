using Stripe;

namespace BoltonCup.WebAPI.Stripe;

public interface IStripeEventConstructor
{
    Event ConstructEvent(string json, string signature, string secret);
}

public class StripeEventConstructor : IStripeEventConstructor
{
    public Event ConstructEvent(string json, string signature, string secret)
        => EventUtility.ConstructEvent(json, signature, secret);
}
