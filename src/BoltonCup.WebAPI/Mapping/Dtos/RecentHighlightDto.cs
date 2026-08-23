namespace BoltonCup.WebAPI.Mapping;

/// <summary>DTO representing a game highlight with game context, for the cross-tournament recent-highlights feed.</summary>
/// <param name="Highlight">The highlight video details.</param>
/// <param name="GameId">The ID of the game the highlight belongs to.</param>
/// <param name="GameTime">The scheduled time of the game.</param>
/// <param name="TournamentName">The name of the tournament the game belongs to.</param>
public sealed record RecentHighlightDto(
    GameHighlightDto Highlight,
    int GameId,
    DateTime GameTime,
    string TournamentName
);
