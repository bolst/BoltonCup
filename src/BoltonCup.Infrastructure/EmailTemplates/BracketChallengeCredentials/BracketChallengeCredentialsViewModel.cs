namespace BoltonCup.Infrastructure.EmailTemplates;

public class BracketChallengeCredentialsViewModel : EmailAuthBaseViewModel
{
    public required string Title { get; set; }
    public required string Link { get; set; }
    public required string Password { get; set; }
}