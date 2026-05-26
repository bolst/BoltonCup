using System.ComponentModel.DataAnnotations;
using BoltonCup.Core.Values;
using FluentValidation;

namespace BoltonCup.WebAPI.Mapping;

/// <summary>Request to complete a new user account with personal details.</summary>
public record CompleteUserAccountRequest
{
    /// <summary>Gets or sets the user's first name.</summary>
    [Required(ErrorMessage = "First name is required.")]
    [MaxLength(50, ErrorMessage = "First name cannot exceed 50 characters.")]
    [Display(Name = "First name")]
    public required string FirstName { get; set; }

    /// <summary>Gets or sets the user's last name.</summary>
    [Required(ErrorMessage = "Last name is required.")]
    [MaxLength(50)]
    [Display(Name = "Last name")]
    public required string LastName { get; set; }

    /// <summary>Gets or sets the user's date of birth.</summary>
    [Required(ErrorMessage = "Birthday is required.")]
    public DateTime Birthday { get; set; }

    /// <summary>Gets or sets the feet component of the user's height.</summary>
    [Range(1, 8)]
    [Required(ErrorMessage = "Height is required.")]
    public int HeightFeet { get; set; }

    /// <summary>Gets or sets the inches component of the user's height.</summary>
    [Range(0, 11)]
    [Required(ErrorMessage = "Height is required.")]
    public int HeightInches { get; set; }

    /// <summary>Gets or sets the user's weight in pounds.</summary>
    [Required(ErrorMessage = "Weight is required.")]
    [Range(0, 1000000, ErrorMessage = "No")]
    public int Weight { get; set; }

    /// <summary>Gets or sets the highest level of hockey the user has played.</summary>
    [Display(Name = "Highest level played")]
    public string? HighestLevel { get; set; }

    /// <summary>Gets or sets the user's preferred beer.</summary>
    [MaxLength(100)]
    [Display(Name = "Preferred beer")]
    public string? PreferredBeer { get; set; }
}

/// <summary>Validates a <see cref="CompleteUserAccountRequest"/>.</summary>
public class CompleteUserAccountRequestValidator : AbstractValidator<CompleteUserAccountRequest>
{
    /// <summary>Initializes a new instance of <see cref="CompleteUserAccountRequestValidator"/>.</summary>
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