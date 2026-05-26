namespace BoltonCup.Core.Commands;

public sealed record UpdateGoalCommand(
    int GameId,
    int GoalId,
    int TeamId,
    int Period,
    string PeriodLabel,
    TimeSpan PeriodTimeRemaining,
    int GoalPlayerId,
    int? Assist1PlayerId,
    int? Assist2PlayerId,
    string? Notes
);
