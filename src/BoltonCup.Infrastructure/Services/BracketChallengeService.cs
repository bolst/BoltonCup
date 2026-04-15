using BoltonCup.Core;
using BoltonCup.Core.BracketChallenge;
using BoltonCup.Core.Exceptions;
using BoltonCup.Infrastructure.Data;
using BoltonCup.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Stripe;

namespace BoltonCup.Infrastructure.Services;

public class BracketChallengeService(
    BoltonCupDbContext _dbContext,
    ILogger<BracketChallengeService> _logger
) : IBracketChallengeService
{

    public async Task<IPagedList<Core.BracketChallenge.Event>> GetBracketChallengesAsync(GetBracketChallengesQuery query, 
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.BracketChallenges
            .AsNoTracking()
            .OrderByDescending(b => b.Title)
            .ToPagedListAsync(query, cancellationToken: cancellationToken);
    }
    
    public async Task<BracketChallengePaymentIntent> CreatePaymentIntentAsync(
        CreateBracketChallengePaymentIntentCommand command, CancellationToken cancellationToken = default)
    {
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
        var adjustedAmount = FeeCalculator.GetAdjustedStripeAmount(registrationFeeAmount);
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
                { nameof(PurchaseType), PurchaseType.BracketChallengeRegistration },
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


    public async Task ProcessPaymentIntentAsync(ProcessBracketChallengePaymentIntentCommand command, CancellationToken cancellationToken = default)
    {
        _dbContext.BracketChallengeRegistrations.Add(new Core.BracketChallenge.Registration
        {
            EventId = command.EventId,
            Name = command.Name,
            Email = command.Email,
            PaymentId = command.PaymentId,
        });
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}