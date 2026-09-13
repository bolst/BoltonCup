using BoltonCup.Core;

namespace BoltonCup.WebAPI.Mapping;

public partial class Mapper
{
    // ---------- SkaterStat ----------

    public GetSkaterStatsQuery ToQuery(GetSkaterStatsRequest request) => new GetSkaterStatsQuery
    {
        TournamentId = request.TournamentId,
        Position = request.Position,
        TeamIds = request.TeamIds,
        GameId = request.GameId,
        Page = request.Page,
        Size = request.Size,
        SortBy = request.SortBy,
        Descending = request.Descending
    };

    public IPagedList<SkaterStatDto> ToDtoList(IPagedList<SkaterStat> skaters) => skaters.ProjectTo(skater => new SkaterStatDto
    {
        PlayerId = skater.PlayerId,
        AccountId = skater.AccountId,
        FirstName = skater.FirstName,
        LastName = skater.LastName,
        Position = skater.Position,
        JerseyNumber = skater.JerseyNumber,
        Birthday = skater.Birthday,
        ProfilePicture = _urlResolver.GetFullUrl(skater.ProfilePicture),
        TeamId = skater.TeamId,
        TeamName = skater.TeamName,
        TeamLogoUrl = _urlResolver.GetFullUrl(skater.TeamLogoUrl),
        TeamAbbreviation = skater.TeamAbbreviation,
        GamesPlayed = skater.GamesPlayed,
        Goals = skater.Goals,
        Assists = skater.Assists,
        Points = skater.Points,
        PenaltyMinutes = skater.PenaltyMinutes,
        PointsPerGame = skater.PointsPerGame
    });
}
