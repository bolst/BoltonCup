using BoltonCup.Core;

namespace BoltonCup.WebAPI.Mapping;

/// <summary>Maps domain models to their corresponding brief DTOs.</summary>
public interface IBriefMapper
{
    /// <summary>Maps a <see cref="DraftPick"/> to a <see cref="DraftPickBriefDto"/>.</summary>
    DraftPickBriefDto? ToDraftPickBriefDto(DraftPick? draftPick);
    /// <summary>Maps a <see cref="Gallery"/> to a <see cref="GalleryBriefDto"/>.</summary>
    GalleryBriefDto ToGalleryBriefDto(Gallery gallery);
    /// <summary>Maps a <see cref="Game"/> to a <see cref="GameBriefDto"/>.</summary>
    GameBriefDto ToGameBriefDto(Game game);
    /// <summary>Maps a <see cref="Goal"/> to a <see cref="GoalBriefDto"/>.</summary>
    GoalBriefDto ToGoalBriefDto(Goal goal);
    /// <summary>Maps an <see cref="InfoGuide"/> to an <see cref="InfoGuideBriefDto"/>.</summary>
    InfoGuideBriefDto ToInfoGuideBriefDto(InfoGuide infoGuide);
    /// <summary>Maps a <see cref="Penalty"/> to a <see cref="PenaltyBriefDto"/>.</summary>
    PenaltyBriefDto ToPenaltyBriefDto(Penalty penalty);
    /// <summary>Maps a <see cref="Player"/> to a <see cref="PlayerBriefDto"/>.</summary>
    PlayerBriefDto ToPlayerBriefDto(Player player);
    /// <summary>Maps a <see cref="Player"/>'s game history to a list of <see cref="PlayerGameByGame"/> entries.</summary>
    List<PlayerGameByGame> ToPlayerGameByGameDtos(Player player);
    /// <summary>Maps a <see cref="Player"/>'s stats to a list of <see cref="PlayerTournamentStats"/> entries.</summary>
    List<PlayerTournamentStats> ToPlayerTournamentStatsDto(Player player);
    /// <summary>Maps a <see cref="Team"/> to a <see cref="TeamBriefDto"/>.</summary>
    TeamBriefDto ToTeamBriefDto(Team team);
    /// <summary>Maps a <see cref="Game"/> to a <see cref="TeamInGameDto"/> for the home or away team.</summary>
    TeamInGameDto? ToTeamInGameDto(Game game, bool home);
    /// <summary>Maps a <see cref="Tournament"/> to a <see cref="TournamentBriefDto"/>.</summary>
    TournamentBriefDto ToTournamentBriefDto(Tournament tournament);
}

/// <summary>Maps domain models to their corresponding brief DTOs.</summary>
public class BriefMapper(IAssetUrlResolver _urlResolver) : IBriefMapper
{
    /// <inheritdoc/>
    public DraftPickBriefDto? ToDraftPickBriefDto(DraftPick? draftPick)
    {
        return draftPick is null
            ? null
            : new DraftPickBriefDto 
            {
                DraftId = draftPick.DraftId,
                OverallPick = draftPick.OverallPick,
                Round = draftPick.Round,
                RoundPick = draftPick.RoundPick,
                Team = ToTeamBriefDto(draftPick.Team),
            };
    }

    /// <inheritdoc/>
    public GalleryBriefDto ToGalleryBriefDto(Gallery gallery)
    {
        return new GalleryBriefDto
        {
            Id = gallery.Id,
            Title = gallery.Title,
            Description = gallery.Description,
            Url = gallery.Source,
        };
    }
    
    /// <inheritdoc/>
    public GameBriefDto ToGameBriefDto(Game game)
    {
        return new GameBriefDto
        {
            Id = game.Id,
            TournamentId = game.TournamentId,
            TournamentName = game.Tournament.Name,
            GameTime = game.GameTime,
            GameType = game.GameType,
            Venue = game.Venue,
            Rink = game.Rink, 
        };
    }
    
    
    /// <inheritdoc/>
    public GoalBriefDto ToGoalBriefDto(Goal goal)
    {
        return new GoalBriefDto
        {
            TimeRemaining = goal.PeriodTimeRemaining,
            Period = goal.Period,
            TeamId = goal.TeamId,
            Scorer = ToPlayerBriefDto(goal.Scorer),
            PrimaryAssist = goal.Assist1Player == null ? null : ToPlayerBriefDto(goal.Assist1Player),
            SecondaryAssist = goal.Assist2Player == null ? null : ToPlayerBriefDto(goal.Assist2Player),
        };
    }


    /// <inheritdoc/>
    public InfoGuideBriefDto ToInfoGuideBriefDto(InfoGuide infoGuide)
    {
        return new InfoGuideBriefDto
        {
            Title = infoGuide.Title,
            MarkdownContent = infoGuide.MarkdownContent,
        };
    }
    
    
    /// <inheritdoc/>
    public PenaltyBriefDto ToPenaltyBriefDto(Penalty penalty)
    {
        return new PenaltyBriefDto
        {
            TimeRemaining = penalty.PeriodTimeRemaining,
            Period = penalty.Period,
            TeamId = penalty.TeamId,
            Player = ToPlayerBriefDto(penalty.Player),
            Infraction = penalty.InfractionName,
            DurationMins = penalty.DurationMinutes
        };
    }
    
    
    /// <inheritdoc/>
    public PlayerBriefDto ToPlayerBriefDto(Player player)
    {
        return new PlayerBriefDto
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
            }
        };
    }


    /// <inheritdoc/>
    public List<PlayerGameByGame> ToPlayerGameByGameDtos(Player player)
    {
        var gameByGames = player.Account.Players
            .SelectMany(p => (
                    (p.Team?.HomeGames ?? []).Select(g => new { Player = p, Game = g, IsHome = true, Opponent = g.AwayTeam })
                )
                .Concat(
                    (p.Team?.AwayGames ?? []).Select(g => new { Player = p, Game = g, IsHome = false, Opponent = g.HomeTeam })
                )
            );
        
        return gameByGames
            .Where(pg => pg.Game.GameState != GameState.Pending)
            .Select(pg => new PlayerGameByGame 
            {
                Goals = pg.Game.Goals.Count(x => x.GoalPlayerId == pg.Player.Id),
                Assists = pg.Game.Goals.Count(x =>
                    x.Assist1PlayerId == pg.Player.Id || x.Assist2PlayerId == pg.Player.Id),
                PenaltyMinutes = pg.Game.Penalties.Where(x => x.PlayerId == pg.Player.Id).Sum(x => x.DurationMinutes),
                Win = pg.Game.Goals.Count(x => x.TeamId == pg.Player.TeamId) >
                      pg.Game.Goals.Count(x => x.TeamId != pg.Player.TeamId),
                Shutouts = pg.Game.Goals.All(x => x.TeamId == pg.Player.TeamId) ? 1 : 0,
                GoalsAgainst = pg.Game.Goals.Count(x => x.TeamId != pg.Player.TeamId),
                Tournament = ToTournamentBriefDto(pg.Game.Tournament),
                Game = new GameOfTeamDto
                {
                    Id = pg.Game.Id,
                    TournamentId = pg.Game.TournamentId,
                    TournamentName = pg.Game.Tournament.Name,
                    GameTime = pg.Game.GameTime,
                    GameType = pg.Game.GameType,
                    Venue = pg.Game.Venue,
                    Rink = pg.Game.Rink,
                    IsHome = pg.IsHome,
                    GoalsFor = pg.Game.Goals.Count(x => x.TeamId == pg.Player.TeamId),
                    GoalsAgainst = pg.Game.Goals.Count(x => x.TeamId != pg.Player.TeamId),
                    Opponent = pg.Opponent == null ? null : ToTeamBriefDto(pg.Opponent),
                },
                Team = pg.Player.Team == null ? null : ToTeamBriefDto(pg.Player.Team)
            })
            .ToList();
    }
    
    
    /// <inheritdoc/>
    public List<PlayerTournamentStats> ToPlayerTournamentStatsDto(Player player)
    {
        return player.Account.Players.GroupBy(p => p.Tournament)
            .Select(g => new PlayerTournamentStats
            {
                GamesPlayed = g.Sum(x => x.SkaterGameLogs.Count + x.GoalieGameLogs.Count),
                Goals = g.Sum(x => x.Goals.Count),
                Assists = g.Sum(x => x.PrimaryAssists.Count + x.SecondaryAssists.Count),
                PenaltyMinutes = g.Sum(x => x.Penalties.Sum(p => p.DurationMinutes)),
                Wins = g.Sum(x => x.GoalieGameLogs.Sum(gl => gl.Wins)),
                Shutouts = g.Sum(x => x.GoalieGameLogs.Sum(gl => gl.Shutouts)),
                GoalsAgainstAverage = g
                    .SelectMany(x => x.GoalieGameLogs)
                    .Select(x => x.GoalsAgainst)
                    .DefaultIfEmpty(0)
                    .Average(),
                Tournament = ToTournamentBriefDto(g.Key),
                Team = g.First().Team == null ? null : ToTeamBriefDto(g.First().Team!),
            })
            .ToList();
    }
    
    
    /// <inheritdoc/>
    public TeamBriefDto ToTeamBriefDto(Team team)
    {
        return new TeamBriefDto
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
    }


    /// <inheritdoc/>
    public TeamInGameDto? ToTeamInGameDto(Game game, bool home)
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
        };
    }
    
    
    /// <inheritdoc/>
    public TournamentBriefDto ToTournamentBriefDto(Tournament tournament)
    {
        return new TournamentBriefDto
        {
            Id = tournament.Id,
            Name = tournament.Name,
            StartDate = tournament.StartDate,
            EndDate = tournament.EndDate,
            WinningTeamId = tournament.WinningTeamId,
            IsActive = tournament.IsActive,
            IsRegistrationOpen = tournament.IsRegistrationOpen,
            Logo = _urlResolver.GetFullUrl(tournament.Logo),
        };
    }
    
    
}