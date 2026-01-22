using System.Linq.Expressions;
using BoltonCup.Core;
using BoltonCup.Core.Mappings;
using BoltonCup.WebAPI.Dtos.Summaries;

namespace BoltonCup.WebAPI.Dtos;

public record PlayerSingleDetailDto : PlayerDetailDto, IMappable<Player, PlayerSingleDetailDto>
{
    private List<PlayerGameByGame> _homeGameByGame { get; init; } = [];
    private List<PlayerGameByGame> _awayGameByGame { get; init; } = [];
    
    public List<PlayerGameByGame> GameByGame =>
        _homeGameByGame.Concat(_awayGameByGame).OrderBy(g => g.Game.GameTime).ToList();

    public required List<PlayerTournamentStats> TournamentStats { get; init; } = [];

    static Expression<Func<Player, PlayerSingleDetailDto>> IMappable<Player, PlayerSingleDetailDto>.Projection =>
        player => new PlayerSingleDetailDto
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
            Tournament = new TournamentSummary(player.Tournament),
            Team = player.Team == null ? null : new TeamSummary(player.Team),
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
                    GoalsAgainstAverage = g.SelectMany(x => x.GoalieGameLogs).Average(x => x.GoalsAgainst),
                    Tournament = new TournamentSummary(g.Key),
                    Team = g.First().Team == null ? null : new TeamSummary(g.First().Team!),
                })
                .ToList(),
            _homeGameByGame = player.Account.Players
                .SelectMany(p => p.Team!.HomeGames.Select(g => new PlayerGameByGame
                {
                    Goals = g.Goals.Count(x => x.GoalPlayerId == p.Id),
                    Assists = g.Goals.Count(x => x.Assist1PlayerId == p.Id || x.Assist2PlayerId == p.Id),
                    PenaltyMinutes = g.Penalties.Where(x => x.PlayerId == p.Id).Sum(x => x.DurationMinutes),
                    Win = g.Goals.Count(x => x.TeamId == p.TeamId) > g.Goals.Count(x => x.TeamId != p.TeamId),
                    Shutouts = g.Goals.All(x => x.TeamId == p.TeamId) ? 1 : 0,
                    GoalsAgainst = g.Goals.Count(x => x.TeamId != p.TeamId),
                    Tournament = new TournamentSummary(g.Tournament),
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
                    Goals = g.Goals.Count(x => x.GoalPlayerId == p.Id),
                    Assists = g.Goals.Count(x => x.Assist1PlayerId == p.Id || x.Assist2PlayerId == p.Id),
                    PenaltyMinutes = g.Penalties.Where(x => x.PlayerId == p.Id).Sum(x => x.DurationMinutes),
                    Win = g.Goals.Count(x => x.TeamId == p.TeamId) > g.Goals.Count(x => x.TeamId != p.TeamId),
                    Shutouts = g.Goals.All(x => x.TeamId == p.TeamId) ? 1 : 0,
                    GoalsAgainst = g.Goals.Count(x => x.TeamId != p.TeamId),
                    Tournament = new TournamentSummary(g.Tournament),
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
        public required int GamesPlayed { get; set; }
        public required int Goals { get; set; }
        public required int Assists { get; set; }
        public int Points => Goals + Assists;       
        public required int PenaltyMinutes { get; set; }
        public required int Wins { get; set; }
        public required int Shutouts { get; set; }
        public required double? GoalsAgainstAverage { get; set; }
        public required TournamentSummary Tournament { get; set; }
        public required TeamSummary? Team { get; set; }
    }

    public sealed record PlayerGameByGame
    {
        public required int Goals  { get; set; }
        public required int Assists  { get; set; }
        public  int Points => Goals + Assists;
        public required int PenaltyMinutes { get; set; }
        public required bool Win { get; set; }
        public required int Shutouts { get; set; }
        public required int GoalsAgainst { get; set; }
        public required TournamentSummary Tournament { get; set; }
        public required TeamGameSummary Game { get; set; }
        public required TeamSummary? Team { get; set; }
    }
}