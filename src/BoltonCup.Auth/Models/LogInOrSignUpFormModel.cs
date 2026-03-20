using System.ComponentModel.DataAnnotations;

namespace BoltonCup.Auth.Models;

public class LogInOrSignUpFormModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}