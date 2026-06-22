using BoltonCup.Core.Values;

namespace BoltonCup.Core;

public sealed class RosterValidator : IRosterValidator
{
    public RosterValidationResult Validate(IEnumerable<Player> roster, int? skaterLimit, int? goalieLimit)
    {
        var players = roster as IReadOnlyCollection<Player> ?? roster.ToList();

        var goalieCount = players.Count(p => string.Equals(p.Position, Position.Goalie, StringComparison.OrdinalIgnoreCase));
        var skaterCount = players.Count - goalieCount;

        var reasons = new List<string>();

        if (skaterLimit is { } sl && skaterCount > sl)
        {
            reasons.Add($"Roster has {skaterCount} skaters but the limit is {sl}.");
        }

        if (goalieLimit is { } gl && goalieCount > gl)
        {
            reasons.Add($"Roster has {goalieCount} goalies but the limit is {gl}.");
        }

        return reasons.Count == 0
            ? RosterValidationResult.Valid
            : RosterValidationResult.Invalid(reasons);
    }
}
