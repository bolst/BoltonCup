using System.ComponentModel.DataAnnotations;

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