using System.Linq.Expressions;
using System.Text.Json.Serialization;
using BoltonCup.Core;
using BoltonCup.Core.Mappings;
using BoltonCup.WebAPI.Dtos.Summaries;

namespace BoltonCup.WebAPI.Dtos;

public record PlayerSingleDetailDto : PlayerDetailDto, IMappable<Player, PlayerSingleDetailDto>
{

    public List<PlayerTournamentStats> TournamentStats { get; set; } = [];

    public List<PlayerGameByGame> GameByGame =>
        _homeGameByGame.Concat(_awayGameByGame).OrderBy(g => g.Game.GameTime).ToList();

    private List<PlayerGameByGame> _homeGameByGame { get; set; } = [];
    private List<PlayerGameByGame> _awayGameByGame { get; set; } = [];

    static Expression<Func<Player, PlayerSingleDetailDto>> IMappable<Player, PlayerSingleDetailDto>.Projection =>
        player => new PlayerSingleDetailDto
        {
            Id = player.Id, 
            TournamentId = player.TournamentId, 
            AccountId = player.AccountId, 
            Position = player.Position, 
            JerseyNumber = player.JerseyNumber, 
            FirstName = player.Account!.FirstName, 
            LastName = player.Account.LastName, 
            Birthday = player.Account.Birthday, 
            ProfilePicture = player.Account.ProfilePicture, 
            PreferredBeer = player.Account.PreferredBeer, 
            TournamentName = player.Tournament.Name, 
            Team = player.Team == null ? null : new TeamSummary(player.Team),
            TournamentStats = player.Account.Players
                .GroupBy(p => p.Tournament)
                .Select(g => new PlayerTournamentStats
                {
                    TournamentId = g.Key.Id,
                    TournamentName = g.Key.Name,
                    Goals = g.Sum(x => x.Goals.Count),
                    Assists = g.Sum(x => x.PrimaryAssists.Count + x.SecondaryAssists.Count),
                    Team = g.First().Team == null ? null : new TeamSummary(g.First().Team),
                })
                .ToList(),
            _homeGameByGame = player.Account.Players
                .SelectMany(p => p.Team!.HomeGames.Select(g => new PlayerGameByGame
                {
                    TournamentId = g.TournamentId,
                    TournamentName = g.Tournament.Name,
                    Goals = g.Goals.Count(x => x.GoalPlayerId == p.Id),
                    Assists = g.Goals.Count(x => x.Assist1PlayerId == p.Id || x.Assist2PlayerId == p.Id),
                    Game = new TeamGameSummary
                    {
                        Id = g.Id,
                        TournamentId = g.TournamentId,
                        TournamentName = g.Tournament.Name,
                        GameTime = g.GameTime,
                        GameType = g.GameType,
                        Venue = g.Venue,
                        Rink = g.Rink,
                        IsHome = true,
                        GoalsFor = g.Goals.Count(x => x.TeamId == p.TeamId),
                        GoalsAgainst = g.Goals.Count(x => x.TeamId != p.TeamId),
                        Opponent = g.AwayTeam == null ? null : new TeamSummary(g.AwayTeam),
                    },
                    Team = new TeamSummary(p.Team)
                }))
                .ToList(),            
            _awayGameByGame = player.Account.Players
                .SelectMany(p => p.Team!.AwayGames.Select(g => new PlayerGameByGame
                {
                    TournamentId = g.TournamentId,
                    TournamentName = g.Tournament.Name,
                    Goals = g.Goals.Count(x => x.GoalPlayerId == p.Id),
                    Assists = g.Goals.Count(x => x.Assist1PlayerId == p.Id || x.Assist2PlayerId == p.Id),
                    Game = new TeamGameSummary
                    {
                        Id = g.Id,
                        TournamentId = g.TournamentId,
                        TournamentName = g.Tournament.Name,
                        GameTime = g.GameTime,
                        GameType = g.GameType,
                        Venue = g.Venue,
                        Rink = g.Rink,
                        IsHome = false,
                        GoalsFor = g.Goals.Count(x => x.TeamId == p.TeamId),
                        GoalsAgainst = g.Goals.Count(x => x.TeamId != p.TeamId),
                        Opponent = g.HomeTeam == null ? null : new TeamSummary(g.HomeTeam),
                    },
                    Team = new TeamSummary(p.Team)
                }))
                .ToList(),
        };

    public sealed record PlayerTournamentStats
    {
        public required int TournamentId { get; set; }
        public required string TournamentName { get; set; }
        public required int Goals { get; set; }
        public required int Assists { get; set; }
        public int Points => Goals + Assists;        
        public required TeamSummary? Team { get; set; }
    }

    public sealed record PlayerGameByGame
    {
        public required int TournamentId { get; set; }
        public required string TournamentName { get; set; }
        public required int Goals  { get; set; }
        public required int Assists  { get; set; }
        public  int Points => Goals + Assists;
        public required TeamGameSummary Game { get; set; }
        public required TeamSummary? Team { get; set; }
    }
}