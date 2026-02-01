namespace BoltonCup.Core;

public class InfoGuide : EntityBase
{
    public required Guid Id { get; set; }
    public string? Title { get; set; }
    public string? MarkdownContent { get; set; }
    public int? TournamentId { get; set; }
    
    public Tournament? Tournament { get; set; }
}