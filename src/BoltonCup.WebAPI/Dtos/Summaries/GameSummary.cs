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


public record TeamGameSummary : GameSummary
{
    public required bool IsHome { get; init; }
    public required int GoalsFor { get; init; }
    public required int GoalsAgainst { get; init; }
    public required TeamSummary? Opponent { get; init; }
}