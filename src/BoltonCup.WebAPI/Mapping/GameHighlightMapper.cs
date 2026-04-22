using BoltonCup.Core;

namespace BoltonCup.WebAPI.Mapping;

public interface IGameHighlightMapper
{
    GameHighlightDto ToDto(GameHighlight highlight);
}

public class GameHighlightMapper(
    IAssetUrlResolver _urlResolver, 
    IBriefMapper _briefMapper
) : IGameHighlightMapper
{
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
