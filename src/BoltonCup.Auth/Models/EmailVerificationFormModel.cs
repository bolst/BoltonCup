using System.ComponentModel.DataAnnotations;

namespace BoltonCup.Auth.Models;

public class EmailVerificationFormModel
{
    [Required]
    public string Code { get; set; } = string.Empty;
}