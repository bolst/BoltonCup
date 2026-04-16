using BoltonCup.Core;

namespace BoltonCup.WebAPI.Mapping;

public interface IGameStarMapper
{
    GameStarDto ToDto(GameStar gameStar);
}

public class GameStarMapper(
    IAssetUrlResolver _urlResolver, 
    IBriefMapper _briefMapper
) : IGameStarMapper
{
    public GameStarDto ToDto(GameStar gameStar)
    {
        return new GameStarDto(
            StarRank: gameStar.StarRank,
            Player: _briefMapper.ToPlayerBriefDto(gameStar.Player)
        );
    }
}
