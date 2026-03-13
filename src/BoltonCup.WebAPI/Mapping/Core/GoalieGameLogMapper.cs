using BoltonCup.Core;

namespace BoltonCup.WebAPI.Mapping.Core;

public interface IGoalieGameLogMapper
{
    IPagedList<GoalieGameLogDto> ToDtoList(IPagedList<GoalieGameLog> logs);
}

public class GoalieGameLogMapper(IAssetUrlResolver _urlResolver) : IGoalieGameLogMapper
{
    public IPagedList<GoalieGameLogDto> ToDtoList(IPagedList<GoalieGameLog> gameLogs)
    {
        return gameLogs.ProjectTo(gameLog => new GoalieGameLogDto
        {
            Player = new PlayerBriefDto(gameLog.Player, gameLog.Player.Account),
            Team = new TeamBriefDto(gameLog.Team),
            OpponentTeamId = gameLog.OpponentTeamId,
            GameId = gameLog.GameId,
            Goals = gameLog.Goals,
            Assists = gameLog.Assists,
            PenaltyMinutes = gameLog.PenaltyMinutes,
            GoalsAgainst = gameLog.GoalsAgainst,
            GoalsAgainstAverage = gameLog.GoalsAgainst / 60.0,
            ShotsAgainst = gameLog.ShotsAgainst,
            Saves = gameLog.Saves,
            SavePercentage = gameLog.Saves == 0 ? 0 : (double)gameLog.ShotsAgainst / gameLog.Saves,
            Shutout = gameLog.Shutout,
            Win = gameLog.Win,
        });
    }    
}
