using BoltonCup.Core.Values;

namespace BoltonCup.Infrastructure.Services;

public readonly record struct AutoPickCandidate(int PlayerId, double Ranking, string? Position, bool CanPlayEither);

public readonly record struct RosteredPlayer(string? Position, bool CanPlayEither);

public static class SmartAutoPickSelector
{
    public const int TargetForwards = 9;
    public const int TargetDefense = 6;

    // Ranks of bias applied per unit of positional deficit. Higher = stronger steer toward needs.
    public const double PositionNeedWeight = 2.0;

    public static AutoPickCandidate? Select(
        IReadOnlyCollection<AutoPickCandidate> available,
        IReadOnlyCollection<RosteredPlayer> roster,
        int remainingPicks)
    {
        if (available.Count == 0)
        {
            return null;
        }

        var goalies = roster.Count(IsGoalie);
        var dedicatedForwards = roster.Count(r => !r.CanPlayEither && IsForward(r.Position));
        var dedicatedDefense = roster.Count(r => !r.CanPlayEither && IsDefense(r.Position));
        var flex = roster.Count(r => r.CanPlayEither && !IsGoalie(r));

        var hasGoalie = goalies >= 1;

        // The team must end with exactly one goalie: if this is the last slot and none is rostered,
        // the pick is forced to a goalie.
        if (!hasGoalie && remainingPicks <= 1)
        {
            var forcedGoalie = BestRanked(available.Where(c => IsGoalie(c.Position)));
            if (forcedGoalie is not null)
            {
                return forcedGoalie;
            }
            // No goalie available — fall through and pick the best skater rather than return nothing.
        }

        // Never draft a second goalie, and reserve the final slot for a still-needed goalie.
        var skaters = available.Where(c => !IsGoalie(c.Position)).ToList();
        if (skaters.Count == 0)
        {
            return BestRanked(available);
        }

        // Allocate fluid players to the side with the larger remaining deficit.
        var effForwards = dedicatedForwards;
        var effDefense = dedicatedDefense;
        for (var i = 0; i < flex; i++)
        {
            if (TargetForwards - effForwards >= TargetDefense - effDefense)
            {
                effForwards++;
            }
            else
            {
                effDefense++;
            }
        }

        var needForwards = Math.Max(0, TargetForwards - effForwards);
        var needDefense = Math.Max(0, TargetDefense - effDefense);
        var bonusForwards = needForwards * PositionNeedWeight;
        var bonusDefense = needDefense * PositionNeedWeight;

        AutoPickCandidate? best = null;
        var bestEffectiveRank = double.MaxValue;
        foreach (var candidate in skaters)
        {
            var bonus = candidate.CanPlayEither
                ? Math.Max(bonusForwards, bonusDefense)
                : IsDefense(candidate.Position) ? bonusDefense
                : IsForward(candidate.Position) ? bonusForwards
                : 0;

            var effectiveRank = candidate.Ranking - bonus;
            if (best is null
                || effectiveRank < bestEffectiveRank
                || (effectiveRank == bestEffectiveRank && candidate.Ranking < best.Value.Ranking))
            {
                best = candidate;
                bestEffectiveRank = effectiveRank;
            }
        }

        return best;
    }

    private static AutoPickCandidate? BestRanked(IEnumerable<AutoPickCandidate> candidates)
    {
        AutoPickCandidate? best = null;
        foreach (var candidate in candidates)
        {
            if (best is null || candidate.Ranking < best.Value.Ranking)
            {
                best = candidate;
            }
        }
        return best;
    }

    private static bool IsGoalie(RosteredPlayer player) 
        => IsGoalie(player.Position);

    private static bool IsGoalie(string? position) =>
        string.Equals(position, Position.Goalie, StringComparison.OrdinalIgnoreCase);

    private static bool IsForward(string? position) =>
        string.Equals(position, Position.Forward, StringComparison.OrdinalIgnoreCase);

    private static bool IsDefense(string? position) =>
        string.Equals(position, Position.Defense, StringComparison.OrdinalIgnoreCase);
}
