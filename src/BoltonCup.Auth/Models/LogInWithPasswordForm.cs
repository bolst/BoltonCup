using System.ComponentModel.DataAnnotations;

namespace BoltonCup.Auth.Models;

public class LogInWithPasswordForm
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
        
    [Required]
    public string Password { get; set; } = string.Empty;
}