namespace BoltonCup.WebAPI.Mapping;

public sealed record GameHighlightDto(
    string VideoUrl,
    string ThumbnailUrl,
    string? Title,
    string? Description,
    PlayerBriefDto? Player
);