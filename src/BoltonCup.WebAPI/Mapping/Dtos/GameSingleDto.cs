namespace BoltonCup.WebAPI.Mapping;

/// <summary>Detailed DTO for a single game, including goals, penalties, stars, and highlights.</summary>
public sealed record GameSingleDto : GameDto
{
    /// <summary>Gets the list of goals scored in the game.</summary>
    public required List<GoalBriefDto> Goals { get; init; } = [];
    /// <summary>Gets the list of penalties called in the game.</summary>
    public required List<PenaltyBriefDto> Penalties { get; init; } = [];
    /// <summary>Gets the list of game stars.</summary>
    public required List<GameStarDto> Stars { get; init; } = [];
    /// <summary>Gets the list of highlight videos for the game.</summary>
    public required List<GameHighlightDto> Highlights { get; init; } = [];
    /// <summary>Gets the top point, goal, and assist leader from each team.</summary>
    public List<GameStatLeaderDto> PlayersToWatch { get; init; } = [];
}


