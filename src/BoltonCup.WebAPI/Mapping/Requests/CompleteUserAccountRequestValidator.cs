using FluentValidation;
using BoltonCup.Core.Values;

namespace BoltonCup.WebAPI.Mapping;

public class CompleteUserAccountRequestValidator : AbstractValidator<CompleteUserAccountRequest>
{
    public CompleteUserAccountRequestValidator()
    {
        RuleFor(x => x.Birthday)
            .NotEmpty().WithMessage("Birthday is required.")
            .Must(BeAValidAge).WithMessage("You must be 16+ years old to play.");

        RuleFor(x => x.HighestLevel)
            .Must(level => SkillLevel.All.Contains(level))
            .When(x => !string.IsNullOrWhiteSpace(x.HighestLevel))
            .WithMessage($"Highest level must be one of: {string.Join(", ", SkillLevel.All)}");
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