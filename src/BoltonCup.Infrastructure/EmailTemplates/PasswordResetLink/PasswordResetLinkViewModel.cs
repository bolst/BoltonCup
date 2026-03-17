namespace BoltonCup.Infrastructure.EmailTemplates;

public class PasswordResetLinkViewModel : EmailAuthBaseViewModel
{
    public required string ResetLink { get; set; }
}