using BoltonCup.Core;

namespace BoltonCup.WebAPI.Mapping;

public partial class Mapper
{
    // ---------- GoalieStat ----------

    public GetGoalieStatsQuery ToQuery(GetGoalieStatsRequest request) => new GetGoalieStatsQuery
    {
        TournamentId = request.TournamentId,
        TeamIds = request.TeamIds,
        GameId = request.GameId,
        Page = request.Page,
        Size = request.Size,
        SortBy = request.SortBy,
        Descending = request.Descending,
    };

    public IPagedList<GoalieStatDto> ToDtoList(IPagedList<GoalieStat> goalies) => goalies.ProjectTo(goalie => new GoalieStatDto
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
