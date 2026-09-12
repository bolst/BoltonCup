namespace BoltonCup.Integrations.Email.EmailTemplates;

public class PasswordResetLinkViewModel : EmailAuthBaseViewModel
{
    public required string ResetLink { get; set; }
}