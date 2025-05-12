using BoltonCup.Shared.Data;
using Stripe;

namespace BoltonCup.Web.Data;

public class StripeServiceProvider
{
    private readonly IBCData _bcData;
    
    public StripeServiceProvider(string apiKey, IBCData bcData)
    {
        StripeConfiguration.ApiKey = apiKey;
        _bcData = bcData;
    }

    public async Task<RegisterFormModel?> ProcessCheckoutAsync(string checkoutId)
    {
        try
        {
            var service = new Stripe.Checkout.SessionService();
            Stripe.Checkout.Session session = await service.GetAsync(checkoutId);

            var email = session.CustomerDetails?.Email;
            if (string.IsNullOrEmpty(email))
            {
                return null;
            }

            var userData = await _bcData.GetRegistrationByEmailAsync(email);
            if (userData is null)
            {
                return null;
            }
            
            await _bcData.SetUserAsPayedAsync(email);
            
            var tournament = await _bcData.GetCurrentTournamentAsync();
            if (tournament is not null)
            {
                await _bcData.ConfigPlayerProfileAsync(userData, tournament.tournament_id);
            }

            return userData;
        }
        catch (Exception e)
        {
            return null;
        }
    }
    
}