namespace BoltonCup.Core.Commands;

public sealed record CreateGoalCommand(
    int GameId,
    int TeamId,
    int Period,
    string PeriodLabel,
    TimeSpan PeriodTimeRemaining,
    int GoalPlayerId,
    int? Assist1PlayerId,
    int? Assist2PlayerId,
    string? Notes
);
