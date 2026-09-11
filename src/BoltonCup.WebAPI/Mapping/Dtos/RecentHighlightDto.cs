namespace BoltonCup.WebAPI.Mapping;

/// <summary>DTO representing a video highlight with its game context, where it has one.</summary>
/// <param name="Highlight">The highlight video details.</param>
/// <param name="GameId">The ID of the most recent game the highlight is tagged with, if any.</param>
/// <param name="GameTime">The scheduled time of that game, if any.</param>
/// <param name="TournamentName">The name of that game's tournament, if any.</param>
public sealed record RecentHighlightDto(
    GameHighlightDto Highlight,
    int? GameId,
    DateTime? GameTime,
    string? TournamentName
);
