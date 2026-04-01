namespace BoltonCup.WebAPI.Mapping;

public record SkaterStatDto
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
    public string? TeamAbbreviation { get; init; }
    public required int GamesPlayed { get; init; }
    public required int Goals { get; init; }
    public required int Assists { get; init; }
    public required int Points { get; init; }
    public required double PenaltyMinutes { get; init; }
    
    public string FullName => FirstName + " " + LastName;
}