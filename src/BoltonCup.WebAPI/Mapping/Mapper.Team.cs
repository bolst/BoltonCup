using BoltonCup.Core;

namespace BoltonCup.WebAPI.Mapping;

public partial class Mapper
{
    // ---------- Team ----------

    public GetTeamsQuery ToQuery(GetTeamsRequest request) => new GetTeamsQuery
    {
        TournamentId = request.TournamentId,
        Page = request.Page,
        Size = request.Size,
        SortBy = request.SortBy,
        Descending = request.Descending,
    };

    public IPagedList<TeamDto> ToDtoList(IPagedList<Team> teams) => teams.ProjectTo(team => new TeamDto
    {
        Id = team.Id,
        Name = team.Name,
        NameShort = team.NameShort,
        Abbreviation = team.Abbreviation,
        Tournament = ToTournamentBriefDto(team.Tournament),
        LogoUrl = _urlResolver.GetFullUrl(team.Logo),
        BannerUrl = _urlResolver.GetFullUrl(team.Banner),
        PrimaryColorHex = team.PrimaryColorHex,
        SecondaryColorHex = team.SecondaryColorHex,
        TertiaryColorHex = team.TertiaryColorHex,
        GoalSongUrl = _urlResolver.GetFullUrl(team.GoalSongTrack != null ? team.GoalSongTrack.AudioFileKey : null),
        WinSongUrl = _urlResolver.GetFullUrl(team.WinSongTrack != null ? team.WinSongTrack.AudioFileKey : null),
        PenaltySongUrl = _urlResolver.GetFullUrl(team.PenaltySongTrack != null ? team.PenaltySongTrack.AudioFileKey : null),
        GeneralManagers = ToTeamGmDtos(team),
    });

    public TeamSingleDto? ToDto(Team? team) => team is null
            ? null
            : new TeamSingleDto
            {
                Id = team.Id,
                Name = team.Name,
                NameShort = team.NameShort,
                Abbreviation = team.Abbreviation,
                Tournament = ToTournamentBriefDto(team.Tournament),
                LogoUrl = _urlResolver.GetFullUrl(team.Logo),
                BannerUrl = _urlResolver.GetFullUrl(team.Banner),
                PrimaryColorHex = team.PrimaryColorHex,
                SecondaryColorHex = team.SecondaryColorHex,
                TertiaryColorHex = team.TertiaryColorHex,
                GoalSongUrl = _urlResolver.GetFullUrl(team.GoalSongTrack?.AudioFileKey),
                WinSongUrl = _urlResolver.GetFullUrl(team.WinSongTrack?.AudioFileKey),
                PenaltySongUrl = _urlResolver.GetFullUrl(team.PenaltySongTrack?.AudioFileKey),
                GeneralManagers = ToTeamGmDtos(team),
                Players = team.Players
                    .Select(ToPlayerBriefDto)
                    .ToList(),
            };

    List<TeamGmDto> ToTeamGmDtos(Team team)
        => team.GeneralManagers
            .Select(a => new TeamGmDto
            {
                AccountId = a.Id,
                FirstName = a.FirstName,
                LastName = a.LastName,
                ProfilePictureUrl = _urlResolver.GetFullUrl(a.Avatar),
            })
            .ToList();
}
