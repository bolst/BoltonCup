using BoltonCup.Core;

#pragma warning disable CS1591

namespace BoltonCup.WebAPI.Mapping;

public partial class Mapper : IMapper
{
    readonly IAssetUrlResolver _urlResolver;

    public Mapper(IAssetUrlResolver urlResolver)
    {
        _urlResolver = urlResolver;
    }

    // Shared helpers used across multiple sections

    static string AccountName(Account? account)
        => account is null ? string.Empty : $"{account.FirstName} {account.LastName}".Trim();

    static IReadOnlyList<PlayerGameAvailabilityDto> BuildAvailability(TournamentAvailability? availability, int accountId)
    {
        if (availability is null)
        {
            return [];
        }

        availability.ByAccount.TryGetValue(accountId, out var responses);
        return availability.Games
            .Select(game => new PlayerGameAvailabilityDto
            {
                GameId = game.GameId,
                GameTime = game.GameTime,
                Availability = responses is not null && responses.TryGetValue(game.GameId, out var a) ? a : null,
            })
            .ToList();
    }

    PlayerBriefDto ToPlayerBriefDto(Player player) => new PlayerBriefDto
    {
        Id = player.Id,
        AccountId = player.AccountId,
        Position = player.Position,
        JerseyNumber = player.JerseyNumber,
        FirstName = player.Account.FirstName,
        LastName = player.Account.LastName,
        Birthday = player.Account.Birthday,
        ProfilePicture = _urlResolver.GetFullUrl(player.Account.Avatar),
        CaptaincyTag = player.Captaincy switch
        {
            Captaincy.Captain => 'C',
            Captaincy.Alternate => 'A',
            _ => null
        },
        CanPlayEitherPosition = player.CanPlayEitherPosition,
    };

    TeamBriefDto ToTeamBriefDto(Team team) => new TeamBriefDto
    {
        Id = team.Id,
        Name = team.Name,
        NameShort = team.NameShort,
        Abbreviation = team.Abbreviation,
        Logo = _urlResolver.GetFullUrl(team.Logo),
        Banner = _urlResolver.GetFullUrl(team.Banner),
        PrimaryColorHex = team.PrimaryColorHex,
        SecondaryColorHex = team.SecondaryColorHex,
        TertiaryColorHex = team.TertiaryColorHex
    };

    TournamentBriefDto ToTournamentBriefDto(Tournament tournament) => new TournamentBriefDto
    {
        Id = tournament.Id,
        Name = tournament.Name,
        StartDate = tournament.StartDate,
        EndDate = tournament.EndDate,
        WinningTeamId = tournament.WinningTeamId,
        IsActive = tournament.IsActive,
        IsRegistrationOpen = tournament.IsRegistrationOpen,
        IsPlayerInfoOpen = tournament.IsPlayerInfoOpen,
        IsTradingOpen = tournament.IsTradingOpen,
        Logo = _urlResolver.GetFullUrl(tournament.Logo),
    };

    TeamInGameDto? ToTeamInGameDto(Game game, bool home)
    {
        var team = home ? game.HomeTeam : game.AwayTeam;
        return team is null
            ? null
            : new TeamInGameDto
            {
                Id = team.Id,
                Name = team.Name,
                NameShort = team.NameShort,
                Abbreviation = team.Abbreviation,
                Logo = _urlResolver.GetFullUrl(team.Logo),
                Banner = _urlResolver.GetFullUrl(team.Banner),
                Goals = game.Goals.Count(g => g.TeamId == team.Id),
                PrimaryColorHex = team.PrimaryColorHex,
                SecondaryColorHex = team.SecondaryColorHex,
                TertiaryColorHex = team.TertiaryColorHex,
                GoalSongFileKey = team.GoalSongTrack?.AudioFileKey,
                GoalSongOffsetSeconds = team.GoalSongTrack?.OffsetSeconds,
                GoalSongTitle = team.GoalSongTrack?.Title,
                PenaltySongFileKey = team.PenaltySongTrack?.AudioFileKey,
                PenaltySongOffsetSeconds = team.PenaltySongTrack?.OffsetSeconds,
                PenaltySongTitle = team.PenaltySongTrack?.Title,
                WinSongFileKey = team.WinSongTrack?.AudioFileKey,
                WinSongOffsetSeconds = team.WinSongTrack?.OffsetSeconds,
                WinSongTitle = team.WinSongTrack?.Title
            };
    }
}
