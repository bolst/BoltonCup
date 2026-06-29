using BoltonCup.Core;

namespace BoltonCup.WebAPI.Mapping;

/// <summary>Brief summary of a game.</summary>
public record GameBriefDto
{
    /// <summary>Gets or sets the game ID.</summary>
    public required int Id { get; set; }
    /// <summary>Gets or sets the ID of the tournament this game belongs to.</summary>
    public required int TournamentId { get; set; }
    /// <summary>Gets or sets the name of the tournament this game belongs to.</summary>
    public required string TournamentName { get; set; }
    /// <summary>Gets or sets the scheduled game time.</summary>
    public required DateTime GameTime { get; set; }
    /// <summary>Gets or sets the type of game (e.g. regular season, playoff).</summary>
    public required GameType? GameType { get; set; }
    /// <summary>Gets or sets the venue where the game is played.</summary>
    public required string? Venue { get; set; }
    /// <summary>Gets or sets the rink where the game is played.</summary>
    public required string? Rink { get; set; }
}
/// <summary>Brief summary of a game from a specific team's perspective.</summary>
public record GameOfTeamDto : GameBriefDto
{
    /// <summary>Gets whether the team is the home team in this game.</summary>
    public required bool IsHome { get; init; }
    /// <summary>Gets the number of goals scored by the team.</summary>
    public required int GoalsFor { get; init; }
    /// <summary>Gets the number of goals scored against the team.</summary>
    public required int GoalsAgainst { get; init; }
    /// <summary>Gets the opposing team.</summary>
    public required TeamBriefDto? Opponent { get; init; }
}