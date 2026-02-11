namespace BoltonCup.Core;

public class SkaterStat
{
    public required int PlayerId { get; init; }
    public required int GamesPlayed { get; init; }
    public required int Goals { get; init; }
    public required int Assists { get; init; }
    public required int Points { get; init; }
    public required double PenaltyMinutes { get; init; }
    public required int AccountId { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required string? Position { get; init; }
    public required int? JerseyNumber { get; init; }
    public required DateTime Birthday { get; init; }
    public required string? ProfilePicture { get; init; }
    public int TeamId { get; init; }
    public string? TeamName { get; init; }
    public string? TeamNameShort { get; init; }
    public string? TeamAbbreviation { get; init; }
    public string? TeamLogoUrl { get; init; }
    public int OpponentId { get; init; }
    public string? OpponentName { get; init; }
    public string? OpponentNameShort { get; init; }
    public string? OpponentAbbreviation { get; init; }
    public string? OpponentLogoUrl { get; init; }
    public int GameId { get; init; }
    public DateTime GameTime { get; init; }
    public string? GameType { get; init; }
    public string? GameVenue { get; init; }
    public string? GameRink { get; init; }
    public int TournamentId { get; init; }
    public string? TournamentName { get; init; }
    public bool TournamentActive { get; init; }
}