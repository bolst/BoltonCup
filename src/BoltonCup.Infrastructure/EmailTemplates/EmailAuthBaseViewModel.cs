namespace BoltonCup.Infrastructure.EmailTemplates;

public abstract class EmailAuthBaseViewModel
{
    public required string LogoUrl { get; set; }
    
    public int CurrentYear { get; set; } = DateTime.UtcNow.Year;
}