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

    // The more I thought about doing this the more I hated it.
    // But then again, it'll just fucken work.
    // Like I could store height feet and height inches, or do cm and then convert ...
    // but is this not just much more beautiful to look at?
    [AllowedValues(
        "3'1\"", "3'2\"", "3'3\"", "3'4\"", "3'5\"", "3'6\"", "3'7\"", "3'8\"", "3'9\"", "3'10\"", "3'11\"",
        "4'0\"", "4'1\"", "4'2\"", "4'3\"", "4'4\"", "4'5\"", "4'6\"", "4'7\"", "4'8\"", "4'9\"", "4'10\"", "4'11\"",
        "5'0\"", "5'1\"", "5'2\"", "5'3\"", "5'4\"", "5'5\"", "5'6\"", "5'7\"", "5'8\"", "5'9\"", "5'10\"", // Nobody is 5'11"
        "6'0\"", "6'1\"", "6'2\"", "6'3\"", "6'4\"", "6'5\"", "6'6\"", "6'7\"", "6'8\"", "6'9\"", "6'10\"", "6'11\"",
        "7'0\"",
        ErrorMessage = "Please select a valid height from the list."
    )]
    [Required(ErrorMessage = "Height is required.")]
    public string Height { get; set; }
    
    [Required(ErrorMessage = "Weight is required.")]
    [Range(0, 1000000, ErrorMessage = "No")]
    [DisplayFormat(DataFormatString = "{0}lb")]
    public int Weight { get; set; }

    [MaxLength(50)]
    [Display(Name = "Highest level played")]
    public string? HighestLevel { get; set; }

    [MaxLength(100)]
    [Display(Name = "Preferred beer")]
    public string? PreferredBeer { get; set; }
}