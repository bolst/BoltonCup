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
        return new GameHighlightDto(
            VideoUrl: _urlResolver.GetVideoUrl(highlight.VideoId) ?? string.Empty,
            Title: highlight.Title,
            Description: highlight.Description,
            Player: highlight.Player is null ? null : _briefMapper.ToPlayerBriefDto(highlight.Player)
        );
    }
}
