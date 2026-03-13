using BoltonCup.Core;

namespace BoltonCup.WebAPI.Mapping.Core;

public interface ISkaterGameLogMapper
{
    IPagedList<SkaterGameLogDto> ToDtoList(IPagedList<SkaterGameLog> logs);
}

public class SkaterGameLogMapper(IAssetUrlResolver _urlResolver) : ISkaterGameLogMapper
{
    public IPagedList<SkaterGameLogDto> ToDtoList(IPagedList<SkaterGameLog> gameLogs)
    {
        return gameLogs.ProjectTo(gameLog => new SkaterGameLogDto
        {
            Player = new PlayerBriefDto(gameLog.Player, gameLog.Player.Account),
            Team = new TeamBriefDto(gameLog.Team),
            OpponentTeamId = gameLog.OpponentTeamId,
            GameId = gameLog.GameId,
            Goals = gameLog.Goals,
            Assists = gameLog.Assists,
            PenaltyMinutes = gameLog.PenaltyMinutes,
        });
    }    
}
