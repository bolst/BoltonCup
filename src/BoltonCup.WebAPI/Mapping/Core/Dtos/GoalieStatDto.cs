namespace BoltonCup.WebAPI.Mapping.Core;

public record GoalieStatDto
{
    public required int PlayerId { get; init; }
    public required int AccountId { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required string? Position { get; init; }
    public required int? JerseyNumber { get; init; }
    public required DateTime Birthday { get; init; }
    public required string? ProfilePicture { get; init; }
    public int? TeamId { get; init; }
    public string? TeamName { get; init; }
    public string? TeamLogoUrl { get; init; }
    public required int GamesPlayed { get; init; }
    public required int Goals { get; init; }
    public required int Assists { get; init; }
    public required double PenaltyMinutes { get; init; }
    public required int GoalsAgainst { get; init; }
    public required double GoalsAgainstAverage { get; init; }
    public required int ShotsAgainst { get; init; }
    public required int Saves { get; init; }
    public required double SavePercentage { get; init; }
    public required int Shutouts { get; init; }
    public required int Wins { get; init; }
    
    public string FullName => FirstName + " " + LastName;
}