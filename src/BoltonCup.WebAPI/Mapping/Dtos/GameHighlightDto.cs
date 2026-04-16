namespace BoltonCup.WebAPI.Mapping;

public sealed record GameHighlightDto(
    string VideoUrl,
    string? Title,
    string? Description,
    PlayerBriefDto? Player
);