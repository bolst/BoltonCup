namespace BoltonCup.WebAPI.Mapping.Core;

public record InfoGuideDto
{
    public required Guid Id { get; init; }
    public string? Title { get; set; }
    public int? TournamentId { get; set; }
    public TournamentBriefDto? Tournament { get; init; }
}