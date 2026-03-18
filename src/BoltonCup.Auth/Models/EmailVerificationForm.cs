using System.ComponentModel.DataAnnotations;

namespace BoltonCup.Auth.Models;

public class EmailVerificationForm
{
    [Required]
    public string Code { get; set; } = string.Empty;
}