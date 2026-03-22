namespace BoltonCup.WebAPI.Mapping;

public record GameBriefDto
{
    public required int Id { get; set; }
    public required int TournamentId { get; set; }
    public required string TournamentName { get; set; }
    public required DateTime GameTime { get; set; }
    public required string? GameType { get; set; }
    public required string? Venue  { get; set; }
    public required string? Rink { get; set; }
}


public record GameOfTeamDto : GameBriefDto
{
    public required bool IsHome { get; init; }
    public required int GoalsFor { get; init; }
    public required int GoalsAgainst { get; init; }
    public required TeamBriefDto? Opponent { get; init; }
}