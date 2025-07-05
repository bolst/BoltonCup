using System.ComponentModel.DataAnnotations;

namespace BoltonCup.Shared.Data;

public class LoginFormModel
{
    [Required(ErrorMessage="Email is required")] 
    public string Email { get; set; }

    [Required(ErrorMessage="Password is required")] 
    public string Password { get; set; }
}