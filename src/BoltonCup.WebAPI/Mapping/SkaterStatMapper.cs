using BoltonCup.Core;

namespace BoltonCup.WebAPI.Mapping;

/// <summary>Maps <see cref="SkaterStat"/> entities to DTOs and queries.</summary>
public interface ISkaterStatMapper
{
    /// <summary>Maps a <see cref="GetSkaterStatsRequest"/> to a <see cref="GetSkaterStatsQuery"/>.</summary>
    GetSkaterStatsQuery ToQuery(GetSkaterStatsRequest request);

    /// <summary>Maps a paged list of <see cref="SkaterStat"/> entities to a paged list of <see cref="SkaterStatDto"/>.</summary>
    IPagedList<SkaterStatDto> ToDtoList(IPagedList<SkaterStat> skaters);
}

/// <summary>Maps <see cref="SkaterStat"/> entities to DTOs and queries.</summary>
public class SkaterStatMapper(IAssetUrlResolver _urlResolver) : ISkaterStatMapper
{
    /// <inheritdoc/>
    public GetSkaterStatsQuery ToQuery(GetSkaterStatsRequest request)
    {
        return new GetSkaterStatsQuery
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
    }


    /// <inheritdoc/>
    public IPagedList<SkaterStatDto> ToDtoList(IPagedList<SkaterStat> skaters)
    {
        return skaters.ProjectTo(skater => new SkaterStatDto
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
            PenaltyMinutes = skater.PenaltyMinutes
        });
    }    
}
