using System.ComponentModel.DataAnnotations;

namespace BoltonCup.Auth.Models;

public class NewPasswordFormModel
{
    [Required]
    [DataType(DataType.Password)]
    [MinLength(8, ErrorMessage = "Password must have at least 8 characters")]
    public string Password { get; set; } = string.Empty;
    
    [Required]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
    [Display(Name = "Confirm password")]
    public string ConfirmPassword { get; set; } = string.Empty;
}