using System.Linq.Expressions;
using BoltonCup.Core;
using BoltonCup.Core.Mappings;
using BoltonCup.WebAPI.Dtos.Summaries;

namespace BoltonCup.WebAPI.Dtos;

public record GameSingleDetailDto : GameDetailDto, IMappable<Game, GameSingleDetailDto>
{

    public List<GoalSummary> Goals { get; set; } = [];
    public List<PenaltySummary> Penalties { get; set; } = [];

    static Expression<Func<Game, GameSingleDetailDto>> IMappable<Game, GameSingleDetailDto>.Projection =>
        game => new GameSingleDetailDto
        {
            Id = game.Id,
            TournamentId = game.TournamentId,
            TournamentName = game.Tournament.Name,
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
        };


    public sealed record GoalSummary
    {
        public required TimeSpan TimeRemaining { get; set; }
        public required int Period { get; set; }
        public required string PeriodLabel { get; set; }
        public required int TeamId { get; set; }
        public required PlayerSummary Scorer  { get; set; }
        public required PlayerSummary? PrimaryAssist { get; set; }
        public required PlayerSummary? SecondaryAssist { get; set; }
    }
    
    public sealed record PenaltySummary
    {
        public required TimeSpan TimeRemaining { get; set; }
        public required int Period { get; set; }
        public required string PeriodLabel { get; set; }
        public required int TeamId { get; set; }
        public required PlayerSummary Player  { get; set; }
        public required string Infraction { get; set; }
        public required int DurationMins { get; set; }
    }
}