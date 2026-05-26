namespace BoltonCup.Core.Commands;

public sealed record UpdatePenaltyCommand(
    int GameId,
    int PenaltyId,
    int TeamId,
    int Period,
    string PeriodLabel,
    TimeSpan PeriodTimeRemaining,
    int PlayerId,
    string InfractionName,
    int DurationMinutes,
    string? Notes
);
