using BoltonCup.Core.Values;
using FluentValidation;

namespace BoltonCup.WebAPI.Mapping;

public record UpdateAccountRequest
{
    public string FirstName { get; set; }

    public string LastName { get; set; }

    public DateTime Birthday { get; set; }

    public string? HighestLevel { get; set; }

    public string? PreferredBeer { get; set; }

    public string? Height { get; set; }
    
    public int? Weight { get; set; }
}



public class UpdateAccountRequestValidator : AbstractValidator<UpdateAccountRequest>
{
    public UpdateAccountRequestValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(50).WithMessage("First name cannot exceed 50 characters.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(50).WithMessage("Last name cannot exceed 50 characters.");

        RuleFor(x => x.Birthday)
            .NotEmpty().WithMessage("Birthday is required.");

        RuleFor(x => x.HighestLevel)
            .Must(level => SkillLevel.All.Contains(level))
            .WithMessage($"Highest level must be one of: {string.Join(", ", SkillLevel.All)}");

        RuleFor(x => x.PreferredBeer)
            .MaximumLength(100);

        RuleFor(x => x.Height)
            .Matches(@"^[1-9]'\s*(1[0-1]|[0-9])$")
            .WithMessage("Height must be in the format FT'IN (e.g., 5'11 or 6'3).")
            .DependentRules(() =>
            {
                RuleFor(x => x.Height)
                    .Must(h =>
                    {
                        if (string.IsNullOrEmpty(h))
                            return true;
                        var parts = h.Split("'");
                        if (parts.Length == 2 && int.TryParse(parts[1].Trim(), out var inches))
                            return inches is >= 0 and < 12;
                        return false;
                    })
                    .WithMessage("Invalid height.");
            });

        RuleFor(x => x.Weight)
            .InclusiveBetween(0, 600).WithMessage("Weight must be between 0 and 600");
    }
}