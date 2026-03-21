namespace BoltonCup.WebAPI.Mapping;

public record GameDto
{
    public required int Id { get; init; }
    public required DateTime GameTime { get; init; }
    public required TournamentBriefDto Tournament { get; init; }
    public string? GameType { get; init; }
    public string? Venue  { get; init; }
    public string? Rink { get; init; }
    public TeamInGameDto? HomeTeam { get; init; }
    public TeamInGameDto? AwayTeam { get; init; }
}