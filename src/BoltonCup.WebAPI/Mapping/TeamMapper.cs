using BoltonCup.Core;

namespace BoltonCup.WebAPI.Mapping;

/// <summary>Maps <see cref="Team"/> entities to DTOs and queries.</summary>
public interface ITeamMapper
{
    /// <summary>Maps a <see cref="GetTeamsRequest"/> to a <see cref="GetTeamsQuery"/>.</summary>
    GetTeamsQuery ToQuery(GetTeamsRequest request);

    /// <summary>Maps a paged list of <see cref="Team"/> entities to a paged list of <see cref="TeamDto"/>.</summary>
    IPagedList<TeamDto> ToDtoList(IPagedList<Team> teams);

    /// <summary>Maps a <see cref="Team"/> to a <see cref="TeamSingleDto"/>, or returns <see langword="null"/> if the team is null.</summary>
    TeamSingleDto? ToDto(Team? team);
}

/// <summary>Maps <see cref="Team"/> entities to DTOs and queries.</summary>
public class TeamMapper(IBriefMapper _briefMapper, IAssetUrlResolver _urlResolver) : ITeamMapper
{
    /// <inheritdoc/>
    public GetTeamsQuery ToQuery(GetTeamsRequest request)
    {
        return new GetTeamsQuery
        {
            TournamentId = request.TournamentId,
            Page = request.Page,
            Size = request.Size,
            SortBy = request.SortBy,
            Descending = request.Descending,
        };
    }
    
    /// <inheritdoc/>
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
            GmFirstName = team.GeneralManager?.FirstName,
            GmLastName = team.GeneralManager?.LastName,
            GmProfilePicture = _urlResolver.GetFullUrl(team.GeneralManager?.Avatar),
        });
    }    
    
    
    /// <inheritdoc/>
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
                GmFirstName = team.GeneralManager?.FirstName,
                GmLastName = team.GeneralManager?.LastName,
                GmProfilePicture = _urlResolver.GetFullUrl(team.GeneralManager?.Avatar),
                Players = team.Players
                    .Select(_briefMapper.ToPlayerBriefDto)
                    .ToList(),
            };
    }
}
