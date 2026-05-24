using FluentValidation;

namespace BoltonCup.WebAPI.Mapping;

/// <summary>Request to create a Stripe payment intent for a bracket challenge entry.</summary>
public record CreateBracketChallengePaymentIntentRequest
{
    /// <summary>Gets or sets the entrant's display name.</summary>
    public required string Name { get; set; }

    /// <summary>Gets or sets the entrant's email address.</summary>
    public required string Email { get; set; }

    /// <summary>Gets or sets whether the entrant has agreed to the terms of service.</summary>
    public bool AgreedToTOS { get; set; }
}

/// <summary>Validates a <see cref="CreateBracketChallengePaymentIntentRequest"/>.</summary>
public class CreateBracketChallengePaymentIntentRequestValidator : AbstractValidator<CreateBracketChallengePaymentIntentRequest>
{
    /// <summary>Initializes a new instance of <see cref="CreateBracketChallengePaymentIntentRequestValidator"/>.</summary>
    public CreateBracketChallengePaymentIntentRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email address.");
        
        RuleFor(x => x.AgreedToTOS)
            .Must(x => x).WithMessage("You must accept the TOS to register.");
    }
}