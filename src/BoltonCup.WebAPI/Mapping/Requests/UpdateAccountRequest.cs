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

    [RegularExpression(@"^[1-9]'\s*(1[0-1]|[0-9])$", ErrorMessage = "Invalid format")]
    public string? Height { get; set; }
    
    [Range(0, 600, ErrorMessage = "Weight must be between 0 and 600")]
    public int? Weight { get; set; }

}