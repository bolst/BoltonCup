using BoltonCup.Core;

namespace BoltonCup.WebAPI.Mapping;

/// <summary>Maps <see cref="GameHighlight"/> entities to DTOs.</summary>
public interface IGameHighlightMapper
{
    /// <summary>Maps a <see cref="GameHighlight"/> to a <see cref="GameHighlightDto"/>.</summary>
    GameHighlightDto ToDto(GameHighlight highlight);
}

/// <summary>Maps <see cref="GameHighlight"/> entities to DTOs.</summary>
public class GameHighlightMapper(
    IAssetUrlResolver _urlResolver,
    IBriefMapper _briefMapper
) : IGameHighlightMapper
{
    /// <inheritdoc/>
    public GameHighlightDto ToDto(GameHighlight highlight)
    {
        var highlightUrls = _urlResolver.GetHighlightUrls(highlight.VideoId);
        return new GameHighlightDto(
            VideoUrl: highlightUrls?.VideoUrl ?? string.Empty,
            ThumbnailUrl: highlightUrls?.ThumbnailUrl ?? string.Empty,
            Title: highlight.Title,
            Description: highlight.Description,
            Player: highlight.Player is null ? null : _briefMapper.ToPlayerBriefDto(highlight.Player)
        );
    }
}
