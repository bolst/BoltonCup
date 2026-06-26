using System.ComponentModel.DataAnnotations;

namespace BoltonCup.Auth.Models;

public class NewPasswordFormModel
{
    [Required]
    [DataType(DataType.Password)]
    [MinLength(8, ErrorMessage = "Password must have at least 8 characters")]
    [RegularExpression(@".*[^a-zA-Z0-9].*", ErrorMessage = "Password must contain at least one non-alphanumeric character")]
    public string Password { get; set; } = string.Empty;
    
    [Required]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
    [Display(Name = "Confirm password")]
    public string ConfirmPassword { get; set; } = string.Empty;
}