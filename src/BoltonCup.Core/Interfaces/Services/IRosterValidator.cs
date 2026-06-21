namespace BoltonCup.Core;

/// <summary>Validates that a team's roster is legal given a tournament's skater/goalie limits.</summary>
public interface IRosterValidator
{
    /// <summary>
    /// Validates a roster against the optional skater and goalie limits. A null limit means unlimited.
    /// Skaters are players whose <see cref="Player.Position"/> is not "goalie"; goalies are players whose position is "goalie".
    /// </summary>
    RosterValidationResult Validate(IEnumerable<Player> roster, int? skaterLimit, int? goalieLimit);
}

public sealed record RosterValidationResult(bool IsValid, IReadOnlyList<string> Reasons)
{
    public static RosterValidationResult Valid { get; } = new(true, []);

    public static RosterValidationResult Invalid(IEnumerable<string> reasons) => new(false, reasons.ToList());
}
