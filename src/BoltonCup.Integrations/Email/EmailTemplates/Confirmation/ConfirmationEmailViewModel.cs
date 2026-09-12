namespace BoltonCup.Integrations.Email.EmailTemplates;

public class ConfirmationEmailViewModel : EmailAuthBaseViewModel
{
    public required string ConfirmationLink { get; set; }
}