using System.Linq.Expressions;
using BoltonCup.Core;
using BoltonCup.Core.Mappings;
using BoltonCup.WebAPI.Dtos.Summaries;

namespace BoltonCup.WebAPI.Dtos;

public record GameSingleDetailDto : GameDetailDto, IMappable<Game, GameSingleDetailDto>
{

    public required List<GoalSummary> Goals { get; init; } = [];
    public required List<PenaltySummary> Penalties { get; init; } = [];
    public required List<GameLogSummary> SkaterStats { get; init; } = [];
    public required List<GameLogSummary> GoalieStats { get; init; } = [];

    static Expression<Func<Game, GameSingleDetailDto>> IMappable<Game, GameSingleDetailDto>.Projection =>
        game => new GameSingleDetailDto
        {
            Id = game.Id,
            Tournament = new TournamentSummary(game.Tournament),
            GameTime = game.GameTime,
            GameType = game.GameType,
            Venue = game.Venue,
            Rink = game.Rink,
            HomeTeam = game.HomeTeam == null ? null : new GameTeamSummary(game.HomeTeam, game.Goals),
            AwayTeam = game.AwayTeam == null ? null : new GameTeamSummary(game.AwayTeam, game.Goals),
            Goals = game.Goals
                .Select(goal => new GoalSummary
                {
                    TimeRemaining = goal.PeriodTimeRemaining,
                    Period = goal.Period,
                    PeriodLabel = goal.PeriodLabel,
                    TeamId = goal.TeamId,
                    Scorer = new PlayerSummary(goal.Scorer, goal.Scorer.Account),
                    PrimaryAssist = goal.Assist1Player == null ? null : new PlayerSummary(goal.Assist1Player, goal.Assist1Player.Account),
                    SecondaryAssist = goal.Assist2Player == null ? null : new PlayerSummary(goal.Assist2Player, goal.Assist2Player.Account),
                })
                .OrderBy(g => g.Period)
                .ThenByDescending(g => g.TimeRemaining)
                .ToList(),
            Penalties = game.Penalties
                .Select(penalty => new PenaltySummary
                {
                    TimeRemaining = penalty.PeriodTimeRemaining,
                    Period = penalty.Period,
                    PeriodLabel = penalty.PeriodLabel,
                    TeamId = penalty.TeamId,
                    Player = new PlayerSummary(penalty.Player, penalty.Player.Account),
                    Infraction = penalty.InfractionName,
                    DurationMins = penalty.DurationMinutes
                })
                .OrderBy(penalty => penalty.Period)
                .ThenByDescending(penalty => penalty.TimeRemaining)
                .ToList(),
            SkaterStats = game.SkaterGameLogs.Select(s => new GameLogSummary(s, s.Player, s.Player.Account)).ToList(),
            GoalieStats = game.GoalieGameLogs.Select(s => new GameLogSummary(s, s.Player, s.Player.Account)).ToList(),
        };
    
}