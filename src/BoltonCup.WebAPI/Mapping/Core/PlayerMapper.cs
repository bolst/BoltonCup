using BoltonCup.Core;

namespace BoltonCup.WebAPI.Mapping.Core;

public interface IPlayerMapper
{
    IPagedList<PlayerDto> ToDtoList(IPagedList<Player> players);
    PlayerSingleDto? ToDto(Player? player);
}

public class PlayerMapper(IAssetUrlResolver _urlResolver) : IPlayerMapper
{
    public IPagedList<PlayerDto> ToDtoList(IPagedList<Player> players)
    {
        return players.ProjectTo(player => new PlayerDto
        {
            Id = player.Id, 
            AccountId = player.AccountId, 
            Position = player.Position, 
            JerseyNumber = player.JerseyNumber, 
            FirstName = player.Account!.FirstName, 
            LastName = player.Account.LastName, 
            Birthday = player.Account.Birthday, 
            ProfilePicture = player.Account.ProfilePicture, 
            BannerPicture = null,
            PreferredBeer = player.Account.PreferredBeer, 
            Tournament = new TournamentBriefDto(player.Tournament),
            Team = player.Team == null ? null : new TeamBriefDto(player.Team),
        });
    }    
    
    
    public PlayerSingleDto? ToDto(Player? player)
    {
        if (player is null)
            return null;

        var gameByGames = player.Account.Players
            .SelectMany(p => (
                    (p.Team?.HomeGames ?? []).Select(g => new { Player = p, Game = g, IsHome = true, Opponent = g.AwayTeam })
                )
                .Concat(
                    (p.Team?.AwayGames ?? []).Select(g => new { Player = p, Game = g, IsHome = false, Opponent = g.HomeTeam })
                )
            );
        
        return new PlayerSingleDto 
        { 
            Id = player.Id, 
            AccountId = player.AccountId, 
            Position = player.Position, 
            JerseyNumber = player.JerseyNumber, 
            FirstName = player.Account!.FirstName, 
            LastName = player.Account.LastName, 
            Birthday = player.Account.Birthday, 
            ProfilePicture = player.Account.ProfilePicture, 
            BannerPicture = null,
            PreferredBeer = player.Account.PreferredBeer, 
            Tournament = new TournamentBriefDto(player.Tournament),
            Team = player.Team == null ? null : new TeamBriefDto(player.Team),
            TournamentStats = player.Account.Players
                .GroupBy(p => p.Tournament)
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
                    Tournament = new TournamentBriefDto(g.Key),
                    Team = g.First().Team == null ? null : new TeamBriefDto(g.First().Team!),
                })
                .ToList(),
            GameByGame = gameByGames
                .Select(pg => new PlayerGameByGame 
                {
                    Goals = pg.Game.Goals.Count(x => x.GoalPlayerId == pg.Player.Id),
                    Assists = pg.Game.Goals.Count(x => x.Assist1PlayerId == pg.Player.Id || x.Assist2PlayerId == pg.Player.Id),
                    PenaltyMinutes = pg.Game.Penalties.Where(x => x.PlayerId == pg.Player.Id).Sum(x => x.DurationMinutes),
                    Win = pg.Game.Goals.Count(x => x.TeamId == pg.Player.TeamId) > pg.Game.Goals.Count(x => x.TeamId != pg.Player.TeamId),
                    Shutouts = pg.Game.Goals.All(x => x.TeamId == pg.Player.TeamId) ? 1 : 0,
                    GoalsAgainst = pg.Game.Goals.Count(x => x.TeamId != pg.Player.TeamId),
                    Tournament = new TournamentBriefDto(pg.Game.Tournament),
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
                        Opponent = pg.Opponent == null ? null : new TeamBriefDto(pg.Opponent),
                    },
                    Team = pg.Player.Team == null ? null : new TeamBriefDto(pg.Player.Team)
                })
                .ToList(),            
            };
    }
}
