using System.Linq.Expressions;
using BoltonCup.Core;
using BoltonCup.Core.Mappings;
using BoltonCup.WebAPI.Dtos.Summaries;

namespace BoltonCup.WebAPI.Dtos;


public record GameBoxScoreDto : IMappable<Game, GameBoxScoreDto>
{
    public required int Id { get; init; }
    public required DateTime GameTime { get; init; }
    public required TournamentSummary Tournament { get; init; }
    public string? GameType { get; init; }
    public string? Venue  { get; init; }
    public string? Rink { get; init; }
    public GameTeamSummary? HomeTeam { get; init; }
    public GameTeamSummary? AwayTeam { get; init; }
    
    private List<GameLogSummary> _skaterGameLogs { get; init; } = [];
    private List<GameLogSummary> _goalieGameLogs { get; init; } = [];
    public List<GameLogSummary> GameLogs => _skaterGameLogs.Concat(_goalieGameLogs).ToList();
    
    static Expression<Func<Game, GameBoxScoreDto>> IMappable<Game, GameBoxScoreDto>.Projection =>
        game => new GameBoxScoreDto
        {
            Id = game.Id,
            Tournament = new TournamentSummary(game.Tournament),
            GameTime = game.GameTime,
            GameType = game.GameType,
            Venue = game.Venue, 
            Rink = game.Rink,
            HomeTeam = game.HomeTeam == null ? null : new GameTeamSummary(game.HomeTeam, game.Goals),
            AwayTeam = game.AwayTeam == null ? null : new GameTeamSummary(game.AwayTeam, game.Goals),
            _skaterGameLogs = game.SkaterGameLogs.Select(s => new GameLogSummary(s, s.Player, s.Player.Account)).ToList(),
            _goalieGameLogs = game.GoalieGameLogs.Select(g => new GameLogSummary(g, g.Player, g.Player.Account)).ToList()
        };
}