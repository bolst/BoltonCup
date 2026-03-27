using BoltonCup.Core;
using BoltonCup.Core.Commands;
using BoltonCup.Infrastructure.Data;
using Stripe;

namespace BoltonCup.Infrastructure.Services;

public class TournamentPaymentService(
    BoltonCupDbContext _dbContext
) : ITournamentPaymentService
{
    public async Task<TournamentPaymentIntent> CreateTournamentPaymentIntentAsync(
        CreateTournamentPaymentIntentCommand command, CancellationToken cancellationToken = default)
    {
        // ensure tournament exists and has registration open
        if (await _dbContext.Tournaments.FindAsync([command.TournamentId], cancellationToken) is not { } tournament)
            throw new ArgumentException("No tournament with that ID exists");
        if (!tournament.IsRegistrationOpen)
            throw new InvalidOperationException($"Tournament with ID {tournament.Id}'s registration is not open.");
        // ensure account exists
        if (await _dbContext.Accounts.FindAsync([command.AccountId], cancellationToken) is not { } account)
            throw new ArgumentException("No account with that ID exists");
        // ensure tournament has appropriate registration fee
        if ((command.IsGoalie ? tournament.GoalieRegistrationFee : tournament.SkaterRegistrationFee) is not { } registrationFee)
            throw new InvalidOperationException($"Tournament with ID {tournament.Id} does not have a registration fee.");
        
        // create payment intent using Stripe
        var service = new PaymentIntentService();
        var paymentIntent = await service.CreateAsync(new PaymentIntentCreateOptions
        {
            Amount = (long)(registrationFee * 100),
            Currency = "cad",
            AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
            {
                Enabled = true
            },
            ReceiptEmail = account.Email,
            Metadata = new Dictionary<string, string>
            {
                { "AccountId", account.Id.ToString() },
                { "TournamentId", tournament.Id.ToString() }
            }
        }, cancellationToken: cancellationToken);
        
        return new TournamentPaymentIntent(
            AccountId: account.Id,
            TournamentId: tournament.Id,
            Amount: registrationFee,
            Secret: paymentIntent.ClientSecret
        );
    }
}