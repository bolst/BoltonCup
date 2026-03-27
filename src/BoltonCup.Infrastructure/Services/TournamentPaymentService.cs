using BoltonCup.Core;
using BoltonCup.Core.Commands;
using BoltonCup.Infrastructure.Data;
using BoltonCup.Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stripe;

namespace BoltonCup.Infrastructure.Services;

public class TournamentPaymentService(
    BoltonCupDbContext _dbContext,
    IPlayerRepository _playerRepository,
    IOptions<StripeSettings> _stripeSettings,
    ILogger<TournamentPaymentService> _logger
) : ITournamentPaymentService
{
    
    private readonly string _stripeWebhookSecret = _stripeSettings.Value.WebhookSecret;
    
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
                { "TournamentId", tournament.Id.ToString() },
                { "Position", command.Position },
            }
        }, cancellationToken: cancellationToken);
        
        return new TournamentPaymentIntent(
            AccountId: account.Id,
            TournamentId: tournament.Id,
            Amount: registrationFee,
            Secret: paymentIntent.ClientSecret
        );
    }


    public async Task ProcessPaymentIntentAsync(string data, string signature, CancellationToken cancellationToken = default)
    {
        var stripeEvent = EventUtility.ConstructEvent(
            data,
            signature,
            _stripeWebhookSecret
        );

        switch (stripeEvent.Type)
        {
            case EventTypes.PaymentIntentSucceeded:
                var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                await HandleSuccessfulPaymentAsync(paymentIntent!);
                break;
            case EventTypes.PaymentIntentPaymentFailed:
                var failedIntent = stripeEvent.Data.Object as PaymentIntent;
                _logger.LogError("Payment failed for Intent: {FailedIntentId}", failedIntent?.Id);
                break;
            default:
                _logger.LogError("Unhandled event type: {StripeEventType}", stripeEvent.Type);
                break;
        }
    }

    private async Task HandleSuccessfulPaymentAsync(PaymentIntent paymentIntent)
    {
        if (paymentIntent.Metadata.TryGetValue("AccountId", out var accountId)
            && paymentIntent.Metadata.TryGetValue("TournamentId", out var tournamentId)
            && paymentIntent.Metadata.TryGetValue("Position", out var position))
        {
            _logger.LogInformation("Payment succeeded for account ID {AccountId} in tournament ID {TournamentId}", accountId, tournamentId);
            await _playerRepository.AddAsync(new Player
            {
                TournamentId = int.Parse(tournamentId),
                AccountId = int.Parse(accountId),
                Position = position,
            });
        }
        else
        {
            _logger.LogError("Payment intent {PaymentIntentId} succeeded but was missing metadata...", paymentIntent.Id);
        }
    }
}