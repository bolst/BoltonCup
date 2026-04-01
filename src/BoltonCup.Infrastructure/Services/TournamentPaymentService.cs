using BoltonCup.Core;
using BoltonCup.Core.Commands;
using BoltonCup.Core.Exceptions;
using BoltonCup.Infrastructure.Data;
using BoltonCup.Infrastructure.Settings;
using Microsoft.EntityFrameworkCore;
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
        // ensure tournament exists
        var tournament = await _dbContext.Tournaments
                             .Include(p => p.Players)
                             .SingleOrDefaultAsync(t => t.Id == command.TournamentId,
                                 cancellationToken: cancellationToken)
                         ?? throw new EntityNotFoundException(nameof(Tournament), command.TournamentId);
        
        // ensure tournament has registration open
        if (!tournament.IsRegistrationOpen)
            throw new TournamentRegistrationClosedException(tournament.Id);
        
        // ensure tournament has appropriate registration fee
        if ((command.IsGoalie ? tournament.GoalieRegistrationFee : tournament.SkaterRegistrationFee) is not { } registrationFeeAmount)
            throw new InvalidOperationException($"Tournament with ID {tournament.Id} does not have a registration fee.");
        
        // ensure account exists
        if (await _dbContext.Accounts.FindAsync([command.AccountId], cancellationToken) is not { } account)
            throw new EntityNotFoundException(nameof(Core.Account), command.AccountId);
        
        // ensure account is not already in tournament
        if (tournament.Players.Any(x => x.AccountId == account.Id))
            throw new AccountAlreadyInTournamentException(account.Id, tournament.Id);
        
        // create payment intent using Stripe
        var service = new PaymentIntentService();
        var adjustedAmount = GetAdjustedStripeAmount(registrationFeeAmount);
        var paymentIntent = await service.CreateAsync(new PaymentIntentCreateOptions
        {
            Amount = (long)(adjustedAmount * 100),
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
            Currency: "CAD",
            TournamentId: tournament.Id,
            Amount: adjustedAmount,
            Secret: paymentIntent.ClientSecret,
            AmountBreakdown: 
            [
                new PaymentBreakdown(
                    Amount: registrationFeeAmount,
                    Title: "Tournament Registration Fee"
                ),
                new PaymentBreakdown(
                    Amount: adjustedAmount - registrationFeeAmount, 
                    Title: "Service fee", 
                    Description: "This covers the few services we use to run Bolton Cup."
                )
            ]
        );
    }


    public async Task ProcessPaymentIntentAsync(string data, string signature, CancellationToken cancellationToken = default)
    {
        var stripeEvent = EventUtility.ConstructEvent(
            data,
            signature,
            _stripeWebhookSecret, 
            throwOnApiVersionMismatch: false
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
                PaymentId = paymentIntent.Id
            });
        }
        else
        {
            _logger.LogError("Payment intent {PaymentIntentId} succeeded but was missing metadata...", paymentIntent.Id);
        }
    }

    private static decimal GetAdjustedStripeAmount(decimal price)
    {
        // Stripe charges 2.9% + 0.3c, so we adjust
        return (price + (decimal)0.3) / (decimal)(1 - 0.029);
    }
}