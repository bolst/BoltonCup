namespace BoltonCup.Infrastructure.EmailTemplates;

public class PasswordResetCodeViewModel
{
    public string FirstName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string ResetCode { get; set; } = string.Empty;
    public string Logo { get; set; } = string.Empty;
}