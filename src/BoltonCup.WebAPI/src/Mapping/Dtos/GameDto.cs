using BoltonCup.Core;

namespace BoltonCup.WebAPI.Mapping;

/// <summary>DTO representing a game.</summary>
public record GameDto
{
    /// <summary>Gets the game ID.</summary>
    public required int Id { get; init; }
    /// <summary>Gets the scheduled game time.</summary>
    public required DateTime GameTime { get; init; }
    /// <summary>Gets the tournament this game belongs to.</summary>
    public required TournamentBriefDto Tournament { get; init; }
    /// <summary>Gets the type of game (e.g. regular season, playoff).</summary>
    public GameType GameType { get; init; }
    /// <summary>Gets the current state of the game.</summary>
    public required GameState GameState { get; init; }
    /// <summary>Gets the venue where the game is played.</summary>
    public string? Venue { get; init; }
    /// <summary>Gets the rink where the game is played.</summary>
    public string? Rink { get; init; }
    /// <summary>Gets the home team with their goal count.</summary>
    public TeamInGameDto? HomeTeam { get; init; }
    /// <summary>Gets the away team with their goal count.</summary>
    public TeamInGameDto? AwayTeam { get; init; }
    /// <summary>Gets the placeholder label for the home side when no team is assigned (e.g. "1st", "Winner of SF1").</summary>
    public string? HomeTeamPlaceholder { get; init; }
    /// <summary>Gets the placeholder label for the away side when no team is assigned (e.g. "4th", "Winner of SF2").</summary>
    public string? AwayTeamPlaceholder { get; init; }
}