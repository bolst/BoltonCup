namespace BoltonCup.WebAPI.Mapping;

/// <summary>DTO representing a game highlight video.</summary>
/// <param name="VideoUrl">The URL of the highlight video.</param>
/// <param name="ThumbnailUrl">The URL of the video thumbnail image.</param>
/// <param name="Title">The title of the highlight.</param>
/// <param name="Description">A description of the highlight.</param>
/// <param name="Player">The player featured in the highlight, if any.</param>
public sealed record GameHighlightDto(
    string VideoUrl,
    string ThumbnailUrl,
    string? Title,
    string? Description,
    PlayerBriefDto? Player
);