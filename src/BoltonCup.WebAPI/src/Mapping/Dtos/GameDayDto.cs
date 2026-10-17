namespace BoltonCup.WebAPI.Mapping;

/// <summary>DTO representing a tournament's games on one Eastern-time day.</summary>
public sealed record GameDayDto
{
    /// <summary>Gets the Eastern-time day.</summary>
    public required DateOnly Date { get; init; }
    /// <summary>Gets the tournament ID.</summary>
    public required int TournamentId { get; init; }
    /// <summary>Gets the tournament name.</summary>
    public required string TournamentName { get; init; }
    /// <summary>Gets the URL of the tournament logo.</summary>
    public string? TournamentLogo { get; init; }
    /// <summary>Gets the number of games the tournament plays on this day.</summary>
    public required int GameCount { get; init; }
}