namespace BoltonCup.Core;

/// <summary>
/// Point values awarded from game outcomes. Hardcoded defaults today; structured as a record so a
/// per-tournament configuration can override these later without touching the computation logic.
/// </summary>
public sealed record StandingsRules
{
    public int RegulationWin { get; init; } = 2;
    public int OtSoWin { get; init; } = 2;
    public int OtSoLoss { get; init; } = 1;
    public int Tie { get; init; } = 1;
    public int RegulationLoss { get; init; }

    public static StandingsRules Default { get; } = new();
}
