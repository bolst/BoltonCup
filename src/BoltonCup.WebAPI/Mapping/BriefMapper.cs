using BoltonCup.Core;

namespace BoltonCup.WebAPI.Mapping;

public interface IBriefMapper
{
    GameBriefDto ToGameBriefDto(Game game);
    GoalBriefDto ToGoalBriefDto(Goal goal);
    InfoGuideBriefDto ToInfoGuideBriefDto(InfoGuide infoGuide);
    PenaltyBriefDto ToPenaltyBriefDto(Penalty penalty);
    PlayerBriefDto ToPlayerBriefDto(Player player);
    List<PlayerGameByGame> ToPlayerGameByGameDtos(Player player);
    List<PlayerTournamentStats> ToPlayerTournamentStatsDto(Player player);
    TeamBriefDto ToTeamBriefDto(Team team);
    TeamInGameDto? ToTeamInGameDto(Game game, bool home);
    TournamentBriefDto ToTournamentBriefDto(Tournament tournament);
}

public class BriefMapper(IAssetUrlResolver _urlResolver) : IBriefMapper
{

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
    
    
    public GoalBriefDto ToGoalBriefDto(Goal goal)
    {
        return new GoalBriefDto
        {
            TimeRemaining = goal.PeriodTimeRemaining,
            Period = goal.Period,
            PeriodLabel = goal.PeriodLabel,
            TeamId = goal.TeamId,
            Scorer = ToPlayerBriefDto(goal.Scorer),
            PrimaryAssist = goal.Assist1Player == null ? null : ToPlayerBriefDto(goal.Assist1Player),
            SecondaryAssist = goal.Assist2Player == null ? null : ToPlayerBriefDto(goal.Assist2Player),
        };
    }


    public InfoGuideBriefDto ToInfoGuideBriefDto(InfoGuide infoGuide)
    {
        return new InfoGuideBriefDto
        {
            Title = infoGuide.Title,
            MarkdownContent = infoGuide.MarkdownContent,
        };
    }
    
    
    public PenaltyBriefDto ToPenaltyBriefDto(Penalty penalty)
    {
        return new PenaltyBriefDto
        {
            TimeRemaining = penalty.PeriodTimeRemaining,
            Period = penalty.Period,
            PeriodLabel = penalty.PeriodLabel,
            TeamId = penalty.TeamId,
            Player = ToPlayerBriefDto(penalty.Player),
            Infraction = penalty.InfractionName,
            DurationMins = penalty.DurationMinutes
        };
    }
    
    
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
        
        return gameByGames.Select(pg => new PlayerGameByGame
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
    
    
    public List<PlayerTournamentStats> ToPlayerTournamentStatsDto(Player player)
    {
        return player.Account.Players.GroupBy(p => p.Tournament)
            .Select(g => new PlayerTournamentStats
            {
                GamesPlayed = g.Sum(x => x.SkaterGameLogs.Count + x.GoalieGameLogs.Count),
                Goals = g.Sum(x => x.Goals.Count),
                Assists = g.Sum(x => x.PrimaryAssists.Count + x.SecondaryAssists.Count),
                PenaltyMinutes = g.Sum(x => x.Penalties.Sum(p => p.DurationMinutes)),
                Wins = g.Sum(x => x.GoalieGameLogs.Count(gl => gl.Win)),
                Shutouts = g.Sum(x => x.GoalieGameLogs.Count(gl => gl.Shutout)),
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
        };
    }


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