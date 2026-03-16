using BoltonCup.Core;

namespace BoltonCup.WebAPI.Mapping.Core;

public interface ITeamMapper
{
    IPagedList<TeamDto> ToDtoList(IPagedList<Team> teams);
    TeamSingleDto? ToDto(Team? team);
}

public class TeamMapper(IBriefMapper _briefMapper, IAssetUrlResolver _urlResolver) : ITeamMapper
{
    public IPagedList<TeamDto> ToDtoList(IPagedList<Team> teams)
    {
        return teams.ProjectTo(team => new TeamDto
        {
            Id = team.Id,
            Name = team.Name,
            NameShort = team.NameShort,
            Abbreviation = team.Abbreviation,
            Tournament = _briefMapper.ToTournamentBriefDto(team.Tournament),
            LogoUrl = _urlResolver.GetFullUrl(team.Logo),
            BannerUrl = _urlResolver.GetFullUrl(team.Banner),
            PrimaryColorHex = team.PrimaryColorHex,
            SecondaryColorHex = team.SecondaryColorHex,
            TertiaryColorHex = team.TertiaryColorHex,
            GoalSongUrl = _urlResolver.GetFullUrl(team.GoalSong),
            PenaltySongUrl = _urlResolver.GetFullUrl(team.PenaltySong),
            GmAccountId = team.GmAccountId,
            GmFirstName = team.GeneralManager!.FirstName,
            GmLastName = team.GeneralManager.LastName,
            GmProfilePicture = _urlResolver.GetFullUrl(team.GeneralManager.Avatar),
        });
    }    
    
    
    public TeamSingleDto? ToDto(Team? team)
    {
        return team is null
            ? null
            : new TeamSingleDto 
            { 
                Id = team.Id,
                Name = team.Name,
                NameShort = team.NameShort,
                Abbreviation = team.Abbreviation,
                Tournament = _briefMapper.ToTournamentBriefDto(team.Tournament),
                LogoUrl = _urlResolver.GetFullUrl(team.Logo),
                BannerUrl = _urlResolver.GetFullUrl(team.Banner),
                PrimaryColorHex = team.PrimaryColorHex,
                SecondaryColorHex = team.SecondaryColorHex,
                TertiaryColorHex = team.TertiaryColorHex,
                GoalSongUrl = _urlResolver.GetFullUrl(team.GoalSong),
                PenaltySongUrl = _urlResolver.GetFullUrl(team.PenaltySong),
                GmAccountId = team.GmAccountId,
                GmFirstName = team.GeneralManager!.FirstName,
                GmLastName = team.GeneralManager.LastName,
                GmProfilePicture = _urlResolver.GetFullUrl(team.GeneralManager.Avatar),
                Players = team.Players
                    .Select(_briefMapper.ToPlayerBriefDto)
                    .ToList(),
            };
    }
}
