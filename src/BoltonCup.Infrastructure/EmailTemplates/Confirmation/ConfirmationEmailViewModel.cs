namespace BoltonCup.Infrastructure.EmailTemplates;

public class ConfirmationEmailViewModel
{
    public string FirstName { get; set; } = string.Empty;
    public string ConfirmationLink { get; set; } = string.Empty;
}