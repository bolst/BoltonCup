namespace BoltonCup.Infrastructure.EmailTemplates;

public class ConfirmationCodeViewModel : EmailAuthBaseViewModel
{
    public required string ConfirmationCode { get; set; }
}