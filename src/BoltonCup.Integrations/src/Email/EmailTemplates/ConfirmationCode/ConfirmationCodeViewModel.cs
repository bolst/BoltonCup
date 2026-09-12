namespace BoltonCup.Integrations.Email.EmailTemplates;

public class ConfirmationCodeViewModel : EmailAuthBaseViewModel
{
    public required string ConfirmationCode { get; set; }
}