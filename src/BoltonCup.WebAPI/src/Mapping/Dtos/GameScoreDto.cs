using BoltonCup.Core;

namespace BoltonCup.WebAPI.Mapping;

/// <summary>DTO representing a game on the scoreboard, with its goals and live status.</summary>
public sealed record GameScoreDto : GameDto
{
    /// <summary>Gets the goals scored in the game, in the order they were scored.</summary>
    public required List<GoalBriefDto> Goals { get; init; } = [];
    /// <summary>Gets the scoreboard status: final when completed, live while in progress, otherwise upcoming.</summary>
    public required GameScoreStatus Status { get; init; }
}