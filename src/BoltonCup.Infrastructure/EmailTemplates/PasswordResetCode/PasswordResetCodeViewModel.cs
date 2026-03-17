namespace BoltonCup.Infrastructure.EmailTemplates;

public class PasswordResetCodeViewModel : EmailAuthBaseViewModel
{
    public required string ResetCode { get; set; }
}