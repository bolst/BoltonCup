using BoltonCup.Core;

namespace BoltonCup.WebAPI.Mapping.Core;

public interface ISkaterStatMapper
{
    IPagedList<SkaterStatDto> ToDtoList(IPagedList<SkaterStat> skaters);
}

public class SkaterStatMapper(IAssetUrlResolver _urlResolver) : ISkaterStatMapper
{
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
            GamesPlayed = skater.GamesPlayed,
            Goals = skater.Goals,
            Assists = skater.Assists,
            Points = skater.Points,
            PenaltyMinutes = skater.PenaltyMinutes
        });
    }    
}
