using System.ComponentModel.DataAnnotations;

namespace BoltonCup.WebAPI.Mapping;

public record UpdateAccountRequest
{
    [Required(ErrorMessage = "First name is required.")]
    [MaxLength(50, ErrorMessage = "First name cannot exceed 50 characters.")]
    public string FirstName { get; set; }

    [Required(ErrorMessage = "Last name is required.")]
    [MaxLength(50)]
    public string LastName { get; set; }

    [Required(ErrorMessage = "Birthday is required.")]
    public DateTime Birthday { get; set; }

    [MaxLength(50)]
    public string? HighestLevel { get; set; }

    [MaxLength(100)]
    public string? PreferredBeer { get; set; }

    [Range(2, 7, ErrorMessage = "Height (ft) must be between 2 and 7")]
    public int? HeightFeet { get; set; }
    
    [Range(1, 11, ErrorMessage = "Height (inches) must be between 1 and 11")]
    public int? HeightInches { get; set; }
    
    [Range(0, 600, ErrorMessage = "Weight must be between 0 and 600")]
    public int? Weight { get; set; }

}