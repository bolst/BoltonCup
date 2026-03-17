namespace BoltonCup.Infrastructure.EmailTemplates;

public class PasswordResetLinkViewModel
{
    public string FirstName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string ResetLink { get; set; } = string.Empty;
    public string Logo { get; set; } = string.Empty;
}