using System.ComponentModel.DataAnnotations;
using BoltonCup.Core.Values;
using FluentValidation;

namespace BoltonCup.WebAPI.Mapping;

public record CompleteUserAccountRequest
{
    [Required(ErrorMessage = "First name is required.")]
    [MaxLength(50, ErrorMessage = "First name cannot exceed 50 characters.")]
    [Display(Name = "First name")]
    public string FirstName { get; set; }

    [Required(ErrorMessage = "Last name is required.")]
    [MaxLength(50)]
    [Display(Name = "Last name")]
    public string LastName { get; set; }

    [Required(ErrorMessage = "Birthday is required.")]
    public DateTime Birthday { get; set; }

    [Range(1, 8)]
    [Required(ErrorMessage = "Height is required.")]
    public int HeightFeet { get; set; }
    
    [Range(0, 11)]
    [Required(ErrorMessage = "Height is required.")]
    public int HeightInches { get; set; }
    
    [Required(ErrorMessage = "Weight is required.")]
    [Range(0, 1000000, ErrorMessage = "No")]
    public int Weight { get; set; }

    [Display(Name = "Highest level played")]
    public string? HighestLevel { get; set; }

    [MaxLength(100)]
    [Display(Name = "Preferred beer")]
    public string? PreferredBeer { get; set; }
}

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