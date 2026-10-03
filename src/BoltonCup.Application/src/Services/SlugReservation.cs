using BoltonCup.Core;
using Microsoft.EntityFrameworkCore;

namespace BoltonCup.Application.Services;

/// <summary>Picks unique slugs for a batch of candidates, shared by every entity with a unique slug column.</summary>
static class SlugReservation
{
    /// <summary>
    /// Returns one unique slug per candidate, in order. A candidate keeps its slug when no other row owns it;
    /// otherwise a numeric suffix is appended. Candidates are unique among themselves too.
    /// </summary>
    /// <param name="candidates">The slugs wanted, each with its row's ID (0 when unsaved).</param>
    /// <param name="slugsOwnedByOthers">Given the candidates' saved IDs, queries every slug owned by any other row.</param>
    /// <param name="cancellationToken">Cancels the query.</param>
    public static async Task<IReadOnlyList<string>> ReserveAsync(
        IReadOnlyList<SlugCandidate> candidates,
        Func<List<int>, IQueryable<string>> slugsOwnedByOthers,
        CancellationToken cancellationToken)
    {
        if (candidates.Count == 0)
        {
            return [];
        }

        var candidateIds = candidates.Select(c => c.Id).Where(id => id != 0).ToList();
        var bases = candidates.Select(c => c.Slug).Distinct().ToList();

        // Every slug that could collide with a base or one of its numbered variants, owned by someone else.
        var taken = (await slugsOwnedByOthers(candidateIds).ToListAsync(cancellationToken))
            .Where(slug => bases.Any(b => slug == b || slug.StartsWith(b + "-")))
            .ToHashSet();

        var reserved = new List<string>(candidates.Count);
        foreach (var candidate in candidates)
        {
            var slug = candidate.Slug;
            for (var suffix = 2; taken.Contains(slug); suffix++)
            {
                slug = SlugGenerator.WithSuffix(candidate.Slug, suffix);
            }

            taken.Add(slug);
            reserved.Add(slug);
        }

        return reserved;
    }
}