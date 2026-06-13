namespace BoltonCup.Infrastructure.EmailTemplates;

public class BroadcastEmailViewModel : EmailAuthBaseViewModel
{
    public required string BodyHtml { get; set; }

    public bool UseLayout { get; set; } = true;
}
