using BoltonCup.Core;

namespace BoltonCup.WebAPI.Mapping;

public partial class Mapper
{
    // ---------- Player ----------

    public GetPlayersQuery ToQuery(GetPlayersRequest request) => new GetPlayersQuery
    {
        TournamentId = request.TournamentId,
        TeamId = request.TeamId,
        Page = request.Page,
        Size = request.Size,
        SortBy = request.SortBy,
        Descending = request.Descending,
    };

    public IPagedList<PlayerDto> ToDtoList(IPagedList<Player> players) => players.ProjectTo(player => new PlayerDto
    {
        Id = player.Id,
        AccountId = player.AccountId,
        Position = player.Position,
        JerseyNumber = player.JerseyNumber,
        FirstName = player.Account!.FirstName,
        LastName = player.Account.LastName,
        Birthday = player.Account.Birthday,
        ProfilePicture = _urlResolver.GetFullUrl(player.Account.Avatar),
        BannerPicture = _urlResolver.GetFullUrl(player.Account.Banner),
        PreferredBeer = player.Account.PreferredBeer,
        Tournament = ToTournamentBriefDto(player.Tournament),
        Team = player.Team == null ? null : ToTeamBriefDto(player.Team),
    });

    public PlayerSingleDto? ToDto(Player? player)
    {
        if (player is null)
        {
            return null;
        }

        return new PlayerSingleDto
        {
            Id = player.Id,
            AccountId = player.AccountId,
            Position = player.Position,
            JerseyNumber = player.JerseyNumber,
            FirstName = player.Account!.FirstName,
            LastName = player.Account.LastName,
            Birthday = player.Account.Birthday,
            ProfilePicture = _urlResolver.GetFullUrl(player.Account.Avatar),
            BannerPicture = _urlResolver.GetFullUrl(player.Account.Banner),
            PreferredBeer = player.Account.PreferredBeer,
            Height = player.Account.HeightFeet is null ? null : $"{player.Account.HeightFeet}'{player.Account.HeightInches}",
            Weight = player.Account.Weight,
            Tournament = ToTournamentBriefDto(player.Tournament),
            Team = player.Team == null ? null : ToTeamBriefDto(player.Team),
            TournamentStats = ToPlayerTournamentStatsDto(player),
            GameByGame = ToPlayerGameByGameDtos(player),
            CanPlayEitherPosition = player.CanPlayEitherPosition,
        };
    }

    public DraftPlayerSingleDto? ToDraftPlayerDto(Player? player, TournamentAvailability availability)
    {
        if (ToDto(player) is not { } basePlayer)
        {
            return null;
        }

        return new DraftPlayerSingleDto
        {
            Id = basePlayer.Id,
            AccountId = basePlayer.AccountId,
            Position = basePlayer.Position,
            JerseyNumber = basePlayer.JerseyNumber,
            FirstName = basePlayer.FirstName,
            LastName = basePlayer.LastName,
            Birthday = basePlayer.Birthday,
            ProfilePicture = basePlayer.ProfilePicture,
            BannerPicture = basePlayer.BannerPicture,
            PreferredBeer = basePlayer.PreferredBeer,
            Height = basePlayer.Height,
            Weight = basePlayer.Weight,
            Tournament = basePlayer.Tournament,
            Team = basePlayer.Team,
            TournamentStats = basePlayer.TournamentStats,
            GameByGame = basePlayer.GameByGame,
            CanPlayEitherPosition = basePlayer.CanPlayEitherPosition,
            GameAvailabilities = BuildAvailability(availability, player!.AccountId),
        };
    }

    List<PlayerGameByGame> ToPlayerGameByGameDtos(Player player)
    {
        var gameByGames = player.Account.Players
            .SelectMany(p => (
                    (p.Team?.HomeGames ?? []).Select(g => new
                    {
                        Player = p,
                        Game = g,
                        IsHome = true,
                        Opponent = g.AwayTeam
                    })
                )
                .Concat(
                    (p.Team?.AwayGames ?? []).Select(g => new
                    {
                        Player = p,
                        Game = g,
                        IsHome = false,
                        Opponent = g.HomeTeam
                    })
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

    List<PlayerTournamentStats> ToPlayerTournamentStatsDto(Player player) => player.Account.Players.GroupBy(p => p.Tournament)
            .Select(g => new PlayerTournamentStats
            {
                GamesPlayed = g.Sum(x => x.SkaterGameLogs.Count + x.GoalieGameLogs.Count),
                Goals = g.Sum(x => x.Goals.Count),
                Assists = g.Sum(x => x.PrimaryAssists.Count + x.SecondaryAssists.Count),
                PenaltyMinutes = g.Sum(x => x.Penalties.Sum(p => p.DurationMinutes)),
                Wins = g.Sum(x => x.GoalieGameLogs.Sum(gl => gl.Wins)),
                Shutouts = g.Sum(x => x.GoalieGameLogs.Sum(gl => gl.Shutouts)),
                GoalieGamesPlayed = g.Sum(x => x.GoalieGameLogs.Count),
                // Per-game GoalsAgainstAverage stores goals-against with empty-net goals already removed
                // (see StatisticsRefreshService), so summing/averaging it keeps empty-net goals out of GAA.
                GoalsAgainst = g.Sum(x => x.GoalieGameLogs.Sum(gl => (int)gl.GoalsAgainstAverage)),
                GoalsAgainstAverage = g
                    .SelectMany(x => x.GoalieGameLogs)
                    .Select(x => x.GoalsAgainstAverage)
                    .DefaultIfEmpty(0)
                    .Average(),
                Tournament = ToTournamentBriefDto(g.Key),
                Team = g.First().Team == null ? null : ToTeamBriefDto(g.First().Team!),
            })
            .ToList();
}
