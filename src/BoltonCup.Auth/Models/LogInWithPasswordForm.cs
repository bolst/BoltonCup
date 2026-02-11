using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BoltonCup.Auth.Models;

public class LogInWithPasswordForm
{
    [Required]
    [EmailAddress]
    [ReadOnly(true)]
    public string Email { get; set; } = string.Empty;
        
    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}