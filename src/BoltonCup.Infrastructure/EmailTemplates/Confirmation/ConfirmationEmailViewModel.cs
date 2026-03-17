namespace BoltonCup.Infrastructure.EmailTemplates;

public class ConfirmationEmailViewModel : EmailAuthBaseViewModel
{
    public required string ConfirmationLink { get; set; }
}