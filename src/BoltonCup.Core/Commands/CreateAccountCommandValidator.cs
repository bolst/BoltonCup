using FluentValidation;
using BoltonCup.Core.Values;

namespace BoltonCup.Core.Commands;

public class CreateAccountCommandValidator : AbstractValidator<CreateAccountCommand>
{
    public CreateAccountCommandValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(50).WithMessage("First name cannot exceed 50 characters.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(50).WithMessage("Last name cannot exceed 50 characters.");

        RuleFor(x => x.Birthday)
            .NotEmpty().WithMessage("Birthday is required.")
            .Must(BeAValidAge).WithMessage("You must be between 16+ years old to play.");

        RuleFor(x => x.Height)
            .NotEmpty().WithMessage("Height is required.");

        RuleFor(x => x.Weight)
            .InclusiveBetween(0, 100000).WithMessage("Weight must be between 0 and 100000 ???");

        RuleFor(x => x.HighestLevel)
            .Must(level => SkillLevel.All.Contains(level))
            .When(x => !string.IsNullOrWhiteSpace(x.HighestLevel))
            .WithMessage($"Highest level must be one of: {string.Join(", ", SkillLevel.All)}");

        RuleFor(x => x.PreferredBeer)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.PreferredBeer))
            .WithMessage("What kind of beer has over 100 characters in its name? Use a shorter one.");
    }

    private bool BeAValidAge(DateTime birthday)
    {
        var today = DateTime.Today;
        var yearsOld = today.Year - birthday.Year;
        // Go back to the year in which the person was born in case of a leap year
        if (birthday.Date > today.AddYears(-yearsOld)) 
            yearsOld--;
        return yearsOld is >= 16 and <= 80;
    }
}