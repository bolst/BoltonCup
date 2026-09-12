using BoltonCup.Core;
using BoltonCup.Core.Commands;
using Microsoft.Extensions.Logging;
using Stripe;

namespace BoltonCup.Integrations.Payments;

public class TournamentPaymentService(
    ITournamentRegistrationService _registrationService,
    ILogger<TournamentPaymentService> _logger
) : ITournamentPaymentService
{

    public async Task<TournamentPaymentIntent> CreateTournamentPaymentIntentAsync(
        CreateTournamentPaymentIntentCommand command, CancellationToken cancellationToken = default)
    {
        var preparation = await _registrationService.PrepareTournamentPaymentAsync(
            command.TournamentId, command.AccountId, command.IsGoalie, cancellationToken);

        // create payment intent using Stripe
        var service = new PaymentIntentService();
        var adjustedAmount = FeeCalculator.GetAdjustedStripeAmount(preparation.RegistrationFee);
        var paymentIntent = await service.CreateAsync(new PaymentIntentCreateOptions
        {
            Amount = (long)(adjustedAmount * 100),
            Currency = "cad",
            AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
            {
                Enabled = true
            },
            ReceiptEmail = preparation.AccountEmail,
            Metadata = new Dictionary<string, string>
            {
                { nameof(PurchaseType), PurchaseType.TournamentRegistration },
                { "AccountId", preparation.AccountId.ToString() },
                { "TournamentId", preparation.TournamentId.ToString() },
                { "Position", command.Position },
            }
        }, cancellationToken: cancellationToken);

        return new TournamentPaymentIntent(
            AccountId: preparation.AccountId,
            Currency: "CAD",
            TournamentId: preparation.TournamentId,
            Amount: adjustedAmount,
            Secret: paymentIntent.ClientSecret,
            AmountBreakdown:
            [
                new PaymentBreakdown(
                    Amount: preparation.RegistrationFee,
                    Title: "Tournament Registration Fee"
                ),
                new PaymentBreakdown(
                    Amount: adjustedAmount - preparation.RegistrationFee,
                    Title: "Service fee",
                    Description: "This covers the few services we use to run Bolton Cup."
                )
            ]
        );
    }


    public async Task ProcessPaymentIntentAsync(ProcessTournamentPaymentIntentCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Processing payment for account {AccountId} in tournament {TournamentId}",
            command.AccountId, command.TournamentId);
        await _registrationService.CompleteRegistrationAsync(
            accountId: command.AccountId,
            tournamentId: command.TournamentId,
            paymentId: command.PaymentId,
            cancellationToken: cancellationToken
        );
    }
}