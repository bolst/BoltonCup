using BoltonCup.Core;

namespace BoltonCup.WebAPI.Mapping;

public interface IGoalieGameLogMapper
{
    GetGoalieGameLogsQuery ToQuery(GetGoalieGameLogsRequest request);
    IPagedList<GoalieGameLogDto> ToDtoList(IPagedList<GoalieGameLog> logs);
}

public class GoalieGameLogMapper(IAssetUrlResolver _urlResolver, IBriefMapper _briefMapper) : IGoalieGameLogMapper
{
    public GetGoalieGameLogsQuery ToQuery(GetGoalieGameLogsRequest request)
    {
        return new GetGoalieGameLogsQuery
        {
            GameId = request.GameId,
            HomeOrAway = request.HomeOrAway,
            TeamId = request.TeamId,
            Page = request.Page,
            Size = request.Size,
            SortBy = request.SortBy,
            Descending = request.Descending,
        };
    }
    
    public IPagedList<GoalieGameLogDto> ToDtoList(IPagedList<GoalieGameLog> gameLogs)
    {
        return gameLogs.ProjectTo(gameLog => new GoalieGameLogDto
        {
            Player = _briefMapper.ToPlayerBriefDto(gameLog.Player),
            Team = _briefMapper.ToTeamBriefDto(gameLog.Team),
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
