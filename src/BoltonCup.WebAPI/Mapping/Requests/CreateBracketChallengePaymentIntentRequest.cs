using FluentValidation;

namespace BoltonCup.WebAPI.Mapping;

public record CreateBracketChallengePaymentIntentRequest
{
    public string Name { get; set; }
    
    public string Email { get; set; }
}


public class CreateBracketChallengePaymentIntentRequestValidator : AbstractValidator<CreateBracketChallengePaymentIntentRequest>
{
    public CreateBracketChallengePaymentIntentRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email address.");
    }
}