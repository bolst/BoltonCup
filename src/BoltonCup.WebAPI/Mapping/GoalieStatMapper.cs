using BoltonCup.Core;

namespace BoltonCup.WebAPI.Mapping;

/// <summary>Maps <see cref="GoalieStat"/> entities to DTOs and queries.</summary>
public interface IGoalieStatMapper
{
    /// <summary>Maps a <see cref="GetGoalieStatsRequest"/> to a <see cref="GetGoalieStatsQuery"/>.</summary>
    GetGoalieStatsQuery ToQuery(GetGoalieStatsRequest request);

    /// <summary>Maps a paged list of <see cref="GoalieStat"/> entities to a paged list of <see cref="GoalieStatDto"/>.</summary>
    IPagedList<GoalieStatDto> ToDtoList(IPagedList<GoalieStat> goalies);
}

/// <summary>Maps <see cref="GoalieStat"/> entities to DTOs and queries.</summary>
public class GoalieStatMapper(IAssetUrlResolver _urlResolver) : IGoalieStatMapper
{
    /// <inheritdoc/>
    public GetGoalieStatsQuery ToQuery(GetGoalieStatsRequest request)
    {
        return new GetGoalieStatsQuery
        {
            TournamentId = request.TournamentId,
            TeamIds = request.TeamIds,
            GameId = request.GameId,
            Page = request.Page,
            Size = request.Size,
            SortBy = request.SortBy,
            Descending = request.Descending,
        };
    }
    
    /// <inheritdoc/>
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
            ProfilePicture = _urlResolver.GetFullUrl(goalie.ProfilePicture),
            TeamId = goalie.TeamId,
            TeamName = goalie.TeamName,
            TeamLogoUrl = _urlResolver.GetFullUrl(goalie.TeamLogoUrl),
            TeamAbbreviation = goalie.TeamAbbreviation,
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
