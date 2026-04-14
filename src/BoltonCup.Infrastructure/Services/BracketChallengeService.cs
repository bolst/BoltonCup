using BoltonCup.Core;
using BoltonCup.Core.BracketChallenge;
using BoltonCup.Core.Exceptions;
using BoltonCup.Infrastructure.Data;
using BoltonCup.Infrastructure.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stripe;

namespace BoltonCup.Infrastructure.Services;

public class BracketChallengeService(
    BoltonCupDbContext _dbContext,
    IOptions<StripeSettings> _stripeSettings,
    ILogger<BracketChallengeService> _logger
) : IBracketChallengeService
{
    
    private readonly string? _stripeWebhookSecret = _stripeSettings.Value.BracketChallengeWebhookSecret;
    
    public async Task<BracketChallengePaymentIntent> CreatePaymentIntentAsync(
        CreateBracketChallengePaymentIntentCommand command, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(_stripeWebhookSecret))
            throw new ArgumentNullException("BracketChallengeWebhookSecret");
        
        // ensure bracket challenge exists
        var bracketChallenge = await _dbContext.BracketChallenges
                             .Include(p => p.Registrations)
                             .SingleOrDefaultAsync(t => t.Id == command.BracketChallengeId,
                                 cancellationToken: cancellationToken)
                         ?? throw new EntityNotFoundException(nameof(Core.BracketChallenge.Event), command.BracketChallengeId);
        
        // ensure bracket challenge has registration open
        if (!bracketChallenge.IsOpen)
            throw new BracketChallengeRegistrationClosedException(bracketChallenge.Id);
        
        // ensure bracket challenge has appropriate registration fee
        if (bracketChallenge.Fee is not { } registrationFeeAmount)
            throw new InvalidOperationException($"Bracket Challenge with ID {bracketChallenge.Id} does not have a registration fee.");
        
        // ensure email is not already in bracket challenge
        if (bracketChallenge.Registrations.Any(x => x.Email.Equals(command.Email, StringComparison.CurrentCultureIgnoreCase)))
            throw new EmailAlreadyInBracketChallengeException(command.Email, bracketChallenge.Id);
        
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
            ReceiptEmail = command.Email,
            Metadata = new Dictionary<string, string>
            {
                { "EventId", command.BracketChallengeId.ToString() },
                { "Name", command.Name },
                { "Email", command.Email },
            }
        }, cancellationToken: cancellationToken);
        
        return new BracketChallengePaymentIntent(
            EventId: bracketChallenge.Id,
            Name: command.Name,
            Email: command.Email,
            Amount: adjustedAmount,
            Currency: "CAD",
            Secret: paymentIntent.ClientSecret,
            AmountBreakdown: 
            [
                new PaymentBreakdown(
                    Amount: registrationFeeAmount,
                    Title: "Bracket Challenge Fee"
                ),
                new PaymentBreakdown(
                    Amount: adjustedAmount - registrationFeeAmount, 
                    Title: "Service fee", 
                    Description: "This covers the service we use for handling payments."
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
        if (paymentIntent.Metadata.TryGetValue("EventId", out var eventIdStr)
            && paymentIntent.Metadata.TryGetValue("Name", out var name)
            && paymentIntent.Metadata.TryGetValue("Email", out var email)
            && int.TryParse(eventIdStr, out var eventId))
        {
            _logger.LogInformation("Payment succeeded for email {Email} in bracket challenge ID {EventId}", email, eventId);
            _dbContext.BracketChallengeRegistrations.Add(new Core.BracketChallenge.Registration
            {
                EventId = eventId,
                Name = name,
                Email = email,
                PaymentId = paymentIntent.Id,
            });
            await _dbContext.SaveChangesAsync();
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