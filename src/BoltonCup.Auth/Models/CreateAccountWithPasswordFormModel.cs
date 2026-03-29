using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BoltonCup.Auth.Models;

public class CreateAccountWithPasswordFormModel
{
    [Required]
    [EmailAddress]
    [ReadOnly(true)]
    public string Email { get; set; } = string.Empty;
        
    [Required]
    [DataType(DataType.Password)]
    [MinLength(8, ErrorMessage = "Password must have at least 8 characters")]
    public string Password { get; set; } = string.Empty;
}