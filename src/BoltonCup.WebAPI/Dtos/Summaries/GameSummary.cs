namespace BoltonCup.WebAPI.Dtos.Summaries;

public record GameSummary
{
    public required int Id { get; set; }
    public required int TournamentId { get; set; }
    public required string TournamentName { get; set; }
    public required DateTime GameTime { get; set; }
    public required string? GameType { get; set; }
    public required string? Venue  { get; set; }
    public required string? Rink { get; set; }
}