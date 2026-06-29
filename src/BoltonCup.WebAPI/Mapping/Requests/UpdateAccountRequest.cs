using BoltonCup.Core.Values;
using FluentValidation;

namespace BoltonCup.WebAPI.Mapping;

/// <summary>Request to update an existing user account's profile information.</summary>
public record UpdateAccountRequest
{
    /// <summary>Gets or sets the user's first name.</summary>
    public string FirstName { get; set; } = null!;

    /// <summary>Gets or sets the user's last name.</summary>
    public string LastName { get; set; } = null!;

    /// <summary>Gets or sets the user's date of birth.</summary>
    public DateTime Birthday { get; set; }

    /// <summary>Gets or sets the highest level of hockey the user has played.</summary>
    public string? HighestLevel { get; set; }

    /// <summary>Gets or sets the user's preferred beer.</summary>
    public string? PreferredBeer { get; set; }

    /// <summary>Gets or sets the user's height in FT'IN format (e.g., "5'11").</summary>
    public string? Height { get; set; }

    /// <summary>Gets or sets the user's weight in pounds.</summary>
    public int? Weight { get; set; }
}
/// <summary>Validates an <see cref="UpdateAccountRequest"/>.</summary>
public class UpdateAccountRequestValidator : AbstractValidator<UpdateAccountRequest>
{
    /// <summary>Initializes a new instance of <see cref="UpdateAccountRequestValidator"/>.</summary>
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
            .InclusiveBetween(0, 1000).WithMessage("Weight must be between 0 and 1000");
    }
}