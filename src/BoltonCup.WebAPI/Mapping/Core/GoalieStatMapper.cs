using BoltonCup.Core;

namespace BoltonCup.WebAPI.Mapping.Core;

public interface IGoalieStatMapper
{
    IPagedList<GoalieStatDto> ToDtoList(IPagedList<GoalieStat> goalies);
}

public class GoalieStatMapper(IAssetUrlResolver _urlResolver) : IGoalieStatMapper
{
    public IPagedList<GoalieStatDto> ToDtoList(IPagedList<GoalieStat> goalies)
    {
        return goalies.ProjectTo(goalie => new GoalieStatDto
        {
            PlayerId = goalie.PlayerId,
            AccountId = goalie.AccountId,
            FirstName = goalie.FirstName,
            LastName = goalie.LastName,
            Position = goalie.Position,
            JerseyNumber = goalie.JerseyNumber,
            Birthday = goalie.Birthday,
            ProfilePicture = goalie.ProfilePicture,
            TeamId = goalie.TeamId,
            TeamName = goalie.TeamName,
            TeamLogoUrl = goalie.TeamLogoUrl,
            GamesPlayed = goalie.GamesPlayed,
            Goals = goalie.Goals,
            Assists = goalie.Assists,
            PenaltyMinutes = goalie.PenaltyMinutes,
            GoalsAgainst = goalie.GoalsAgainst,
            GoalsAgainstAverage = goalie.GoalsAgainstAverage,
            ShotsAgainst = goalie.ShotsAgainst,
            Saves = goalie.Saves,
            SavePercentage = goalie.SavePercentage,
            Shutouts = goalie.Shutouts,
            Wins = goalie.Wins
        });
    }    
}
