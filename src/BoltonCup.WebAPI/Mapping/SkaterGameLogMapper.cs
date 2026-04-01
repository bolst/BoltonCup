using BoltonCup.Core;

namespace BoltonCup.WebAPI.Mapping;

public interface ISkaterGameLogMapper
{
    GetSkaterGameLogsQuery ToQuery(GetSkaterGameLogsRequest request);
    IPagedList<SkaterGameLogDto> ToDtoList(IPagedList<SkaterGameLog> logs);
}

public class SkaterGameLogMapper(IBriefMapper _briefMapper, IAssetUrlResolver _urlResolver) : ISkaterGameLogMapper
{
    public GetSkaterGameLogsQuery ToQuery(GetSkaterGameLogsRequest request)
    {
        return new GetSkaterGameLogsQuery
        {
            GameId = request.GameId,
            HomeOrAway = request.HomeOrAway,
            TeamId = request.TeamId,
            Position = request.Position,
            Page = request.Page,
            Size = request.Size,
            SortBy = request.SortBy,
            Descending = request.Descending,
        };
    }
    
    public IPagedList<SkaterGameLogDto> ToDtoList(IPagedList<SkaterGameLog> gameLogs)
    {
        return gameLogs.ProjectTo(gameLog => new SkaterGameLogDto
        {
            Player = _briefMapper.ToPlayerBriefDto(gameLog.Player),
            Team = _briefMapper.ToTeamBriefDto(gameLog.Team),
            OpponentTeamId = gameLog.OpponentTeamId,
            GameId = gameLog.GameId,
            Goals = gameLog.Goals,
            Assists = gameLog.Assists,
            PenaltyMinutes = gameLog.PenaltyMinutes,
        });
    }    
}
