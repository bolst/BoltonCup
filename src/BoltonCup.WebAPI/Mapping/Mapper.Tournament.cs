using BoltonCup.Core;
using BoltonCup.Core.Commands;

namespace BoltonCup.WebAPI.Mapping;

public partial class Mapper
{
    // ---------- Tournament ----------

    public GetTournamentsQuery ToQuery(GetTournamentsRequest request) => new GetTournamentsQuery
    {
        RegistrationOpen = request.RegistrationOpen,
        Page = request.Page,
        Size = request.Size,
        SortBy = request.SortBy,
        Descending = request.Descending,
    };

    public IPagedList<TournamentDto> ToDtoList(IPagedList<Tournament> tournaments) => tournaments.ProjectTo(tournament => new TournamentDto
    {
        Id = tournament.Id,
        Name = tournament.Name,
        Logo = _urlResolver.GetFullUrl(tournament.Logo),
        BackgroundImage = _urlResolver.GetFullUrl(tournament.BackgroundImage),
        StartDate = tournament.StartDate,
        EndDate = tournament.EndDate,
        WinningTeamId = tournament.WinningTeamId,
        IsActive = tournament.IsActive,
        IsRegistrationOpen = tournament.IsRegistrationOpen,
        IsPaymentOpen = tournament.IsPaymentOpen,
        IsPlayerInfoOpen = tournament.IsPlayerInfoOpen,
        IsTradingOpen = tournament.IsTradingOpen,
        SkaterLimit = tournament.SkaterLimit,
        GoalieLimit = tournament.GoalieLimit,
        Gallery = tournament.Gallery is null ? null : ToGalleryBriefDto(tournament.Gallery)
    });

    public TournamentSingleDto? ToDto(Tournament? tournament) => tournament is null
            ? null
            : new TournamentSingleDto
            {
                Id = tournament.Id,
                Name = tournament.Name,
                Logo = _urlResolver.GetFullUrl(tournament.Logo),
                BackgroundImage = _urlResolver.GetFullUrl(tournament.BackgroundImage),
                StartDate = tournament.StartDate,
                EndDate = tournament.EndDate,
                WinningTeamId = tournament.WinningTeamId,
                IsActive = tournament.IsActive,
                IsRegistrationOpen = tournament.IsRegistrationOpen,
                IsPaymentOpen = tournament.IsPaymentOpen,
                IsPlayerInfoOpen = tournament.IsPlayerInfoOpen,
                IsTradingOpen = tournament.IsTradingOpen,
                SkaterLimit = tournament.SkaterLimit,
                GoalieLimit = tournament.GoalieLimit,
                InfoGuide = tournament.InfoGuide is null ? null : ToInfoGuideBriefDto(tournament.InfoGuide),
                Gallery = tournament.Gallery is null ? null : ToGalleryBriefDto(tournament.Gallery),
                Sponsors = tournament.Sponsors
                    .Select(sponsor => new TournamentSponsorDto
                    {
                        Name = sponsor.Name,
                        LogoUrl = _urlResolver.GetFullUrl(sponsor.Logo),
                        WebsiteUrl = sponsor.WebsiteUrl,
                    })
                    .ToList()
            };

    public PlayerStatLeadersDto ToDto(string title, IEnumerable<SkaterStat> stats, Func<SkaterStat, double> selector, string? format = null) => new PlayerStatLeadersDto
    {
        Title = title,
        Leaders = stats.Select(stat => ToPlayerStatDto(stat, selector, format))
    };

    public PlayerStatLeadersDto ToDto(string title, IEnumerable<GoalieStat> stats, Func<GoalieStat, double> selector, string? format = null) => new PlayerStatLeadersDto
    {
        Title = title,
        Leaders = stats.Select(stat => new PlayerStatDto
        {
            PlayerId = stat.PlayerId,
            AccountId = stat.AccountId,
            FirstName = stat.FirstName,
            LastName = stat.LastName,
            Position = stat.Position,
            JerseyNumber = stat.JerseyNumber,
            Birthday = stat.Birthday,
            ProfilePicture = _urlResolver.GetFullUrl(stat.ProfilePicture),
            TeamId = stat.TeamId,
            TeamName = stat.TeamName,
            TeamLogoUrl = _urlResolver.GetFullUrl(stat.TeamLogoUrl),
            TeamAbbreviation = stat.TeamAbbreviation,
            StatValue = selector(stat),
            StatString = selector(stat).ToString(format)
        })
    };

    public GameStatLeaderDto ToGameStatLeaderDto(string title, SkaterStat? home, SkaterStat? away, Func<SkaterStat, double> selector, string? format = null) => new GameStatLeaderDto(
            Title: title,
            HomeLeader: home is null ? null : ToGameStatLeaderPlayerDto(home, selector, format),
            AwayLeader: away is null ? null : ToGameStatLeaderPlayerDto(away, selector, format)
        );

    GameStatLeaderPlayerDto ToGameStatLeaderPlayerDto(SkaterStat stat, Func<SkaterStat, double> selector, string? format = null) => new GameStatLeaderPlayerDto
    {
        PlayerId = stat.PlayerId,
        AccountId = stat.AccountId,
        FirstName = stat.FirstName,
        LastName = stat.LastName,
        Position = stat.Position,
        JerseyNumber = stat.JerseyNumber,
        ProfilePicture = _urlResolver.GetFullUrl(stat.ProfilePicture),
        StatValue = selector(stat),
        StatString = selector(stat).ToString(format)
    };

    PlayerStatDto ToPlayerStatDto(SkaterStat stat, Func<SkaterStat, double> selector, string? format = null) => new PlayerStatDto
    {
        PlayerId = stat.PlayerId,
        AccountId = stat.AccountId,
        FirstName = stat.FirstName,
        LastName = stat.LastName,
        Position = stat.Position,
        JerseyNumber = stat.JerseyNumber,
        Birthday = stat.Birthday,
        ProfilePicture = _urlResolver.GetFullUrl(stat.ProfilePicture),
        TeamId = stat.TeamId,
        TeamName = stat.TeamName,
        TeamLogoUrl = _urlResolver.GetFullUrl(stat.TeamLogoUrl),
        TeamAbbreviation = stat.TeamAbbreviation,
        StatValue = selector(stat),
        StatString = selector(stat).ToString(format)
    };

    GalleryBriefDto ToGalleryBriefDto(Gallery gallery) => new GalleryBriefDto
    {
        Id = gallery.Id,
        Title = gallery.Title,
        Description = gallery.Description,
        Url = gallery.Source,
    };

    InfoGuideBriefDto ToInfoGuideBriefDto(InfoGuide infoGuide) => new InfoGuideBriefDto
    {
        Title = infoGuide.Title,
        MarkdownContent = infoGuide.MarkdownContent,
    };


    // ---------- TournamentPayment ----------

    public TournamentPaymentIntentDto ToDto(TournamentPaymentIntent paymentIntent) => new TournamentPaymentIntentDto(
            ClientSecret: paymentIntent.Secret,
            TotalAmount: paymentIntent.Amount,
            Currency: paymentIntent.Currency,
            Breakdown: paymentIntent.AmountBreakdown
        );

    public CreateTournamentPaymentIntentCommand ToCommand(int tournamentId, int accountId, CreateTournamentPaymentIntentRequest request) => new CreateTournamentPaymentIntentCommand(
            AccountId: accountId,
            TournamentId: tournamentId,
            Position: request.Position
        );


    // ---------- TournamentRegistration ----------

    public TournamentRegistrationDto? ToDto(TournamentRegistration? registration) => registration is null
            ? null
            : new TournamentRegistrationDto
            {
                CurrentStep = registration.CurrentStep,
                Payload = registration.Payload,
                IsComplete = registration.IsComplete,
            };


    // ---------- TournamentPlayerInfo ----------

    public TournamentPlayerInfoDto ToDto(TournamentPlayerInfoContext context) => new TournamentPlayerInfoDto
    {
        GameAvailability = context.Info?.GameAvailabilities
                .Select(a => new GameAvailabilityDto
                {
                    GameId = a.GameId,
                    Availability = a.Availability
                })
                .ToList() ?? [],
        Song = context.Info?.SongTrackId is { } trackId
                ? new MusicTrackDto
                {
                    Id = trackId,
                    Name = context.Info.SongName ?? string.Empty,
                    Artist = context.Info.SongArtist ?? string.Empty,
                    AlbumArtUrl = context.Info.SongAlbumArtUrl,
                }
                : null,
        Games = context.TeamGames.Select(game => new GameDto
        {
            Id = game.Id,
            Tournament = ToTournamentBriefDto(game.Tournament),
            GameTime = game.GameTime,
            GameType = game.GameType,
            GameState = game.GameState,
            Venue = game.Venue,
            Rink = game.Rink,
            HomeTeam = ToTeamInGameDto(game, home: true),
            AwayTeam = ToTeamInGameDto(game, home: false),
            HomeTeamPlaceholder = game.HomeTeamPlaceholder,
            AwayTeamPlaceholder = game.AwayTeamPlaceholder,
        }).ToList(),
        CurrentTeam = context.CurrentTeam is { } currentTeam ? ToTeamBriefDto(currentTeam) : null,
        ManagedTeam = context.ManagedTeam is { } team
                ? new ManagedTeamDto
                {
                    TeamId = team.TeamId,
                    TeamName = team.TeamName,
                    GoalSong = ToMusicTrackDto(team.GoalSongTrack),
                    WinSong = ToMusicTrackDto(team.WinSongTrack),
                    PenaltySong = ToMusicTrackDto(team.PenaltySongTrack),
                }
                : null,
    };

    static MusicTrackDto? ToMusicTrackDto(TournamentMusicTrack? track)
        => track?.TrackId is { } trackId
            ? new MusicTrackDto
            {
                Id = trackId,
                Name = track.Title ?? string.Empty,
                Artist = track.Artist ?? string.Empty,
                AlbumArtUrl = track.AlbumArtUrl,
            }
            : null;
}
