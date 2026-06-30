using BoltonCup.Core.Values;

namespace BoltonCup.Infrastructure.Services;

public readonly record struct AutoPickCandidate(int PlayerId, double Ranking, string? Position, bool CanPlayEither);

public readonly record struct RosteredPlayer(string? Position, bool CanPlayEither);

public static class SmartAutoPickSelector
{
    public const int TargetForwards = 9;
    public const int TargetDefense = 6;

    // Ranks of bias applied per unit of positional deficit. Higher = stronger steer toward needs.
    public const double DefaultPositionNeedWeight = 2.0;

    // Max magnitude of random perturbation (in ranking units) applied to each candidate's effective
    // rank so repeated auto-drafts of the same scenario diverge. Only applied when a Random is supplied.
    public const double DefaultNoiseMagnitude = 1.5;

    public static AutoPickCandidate? Select(
        IReadOnlyCollection<AutoPickCandidate> available,
        IReadOnlyCollection<RosteredPlayer> roster,
        int remainingPicks,
        Random? random = null,
        double? positionNeedWeight = null,
        double? noiseMagnitude = null)
    {
        var posWeight = positionNeedWeight ?? DefaultPositionNeedWeight;
        var noiseMag = noiseMagnitude ?? DefaultNoiseMagnitude;
        if (available.Count == 0)
        {
            return null;
        }

        var hasGoalie = roster.Any(IsGoalie);

        // A team carries exactly one goalie, so once it has one no further goalie is ever eligible.
        var candidates = hasGoalie
            ? available.Where(c => !IsGoalie(c.Position)).ToList()
            : available.ToList();
        if (candidates.Count == 0)
        {
            // Only goalies remain but the team already has one; nothing valid to pick.
            return null;
        }

        // The team must end with exactly one goalie: if this is the last slot and none is rostered,
        // force a goalie over any skater so the roster can't finish goalie-less. This is the case
        // where the team holding the final available goalie naturally takes it with its last pick.
        if (!hasGoalie && remainingPicks <= 1)
        {
            var forcedGoalie = BestRanked(candidates.Where(c => IsGoalie(c.Position)), random, noiseMag);
            if (forcedGoalie is not null)
            {
                return forcedGoalie;
            }
            // No goalie available — fall through and pick the best skater rather than return nothing.
        }

        var dedicatedForwards = roster.Count(r => !r.CanPlayEither && IsForward(r.Position));
        var dedicatedDefense = roster.Count(r => !r.CanPlayEither && IsDefense(r.Position));
        var flex = roster.Count(r => r.CanPlayEither && !IsGoalie(r));

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
        var bonusForwards = needForwards * posWeight;
        var bonusDefense = needDefense * posWeight;
        // Goalie is a position need (target one) like any other, so a goalie is drafted by value
        // across the draft instead of always being deferred to the final pick. The deficit is at
        // most one, so the goalie need is weak relative to deep skater needs and only wins once a
        // team's forward/defense slots are largely filled.
        var bonusGoalie = hasGoalie ? 0 : posWeight;

        AutoPickCandidate? best = null;
        var bestEffectiveRank = double.MaxValue;
        foreach (var candidate in candidates)
        {
            var bonus = IsGoalie(candidate.Position) ? bonusGoalie
                : candidate.CanPlayEither ? Math.Max(bonusForwards, bonusDefense)
                : IsDefense(candidate.Position) ? bonusDefense
                : IsForward(candidate.Position) ? bonusForwards
                : 0;

            var noise = random is null ? 0.0 : (random.NextDouble() - 0.5) * 2.0 * noiseMag;
            var effectiveRank = candidate.Ranking - bonus + noise;
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

    private static AutoPickCandidate? BestRanked(IEnumerable<AutoPickCandidate> candidates, Random? random, double noiseMagnitude)
    {
        AutoPickCandidate? best = null;
        var bestRank = double.MaxValue;
        foreach (var candidate in candidates)
        {
            var noise = random is null ? 0.0 : (random.NextDouble() - 0.5) * 2.0 * noiseMagnitude;
            var rank = candidate.Ranking + noise;
            if (best is null || rank < bestRank)
            {
                best = candidate;
                bestRank = rank;
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
