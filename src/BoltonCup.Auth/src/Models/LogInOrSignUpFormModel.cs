using System.ComponentModel.DataAnnotations;
using BoltonCup.Auth.Attributes;

namespace BoltonCup.Auth.Models;

public class LogInOrSignUpFormModel
{
    [Required]
    [EmailAddress]
    [AutoFocus]
    public string Email { get; set; } = string.Empty;
}