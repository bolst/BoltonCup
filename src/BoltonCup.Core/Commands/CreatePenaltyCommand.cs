namespace BoltonCup.Core.Commands;

public sealed record CreatePenaltyCommand(
    int GameId,
    int TeamId,
    int Period,
    string PeriodLabel,
    TimeSpan PeriodTimeRemaining,
    int PlayerId,
    string InfractionName,
    int DurationMinutes,
    string? Notes
);
