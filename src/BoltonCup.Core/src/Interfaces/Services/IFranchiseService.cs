using BoltonCup.Core.Commands;

namespace BoltonCup.Core;

public interface IFranchiseService
{
    /// <summary>All franchises with title and season counts, sorted by titles descending, then name.</summary>
    Task<IReadOnlyList<FranchiseSummary>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// The owners, titles, all-time record, season history and leaders of the franchise with this exact slug;
    /// null when not found.
    /// </summary>
    Task<FranchiseDetail?> GetDetailBySlugAsync(string slug, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns one unique slug per candidate, in order. A candidate keeps its slug when no other
    /// franchise owns it; otherwise a numeric suffix is appended. Candidates are unique among themselves too.
    /// </summary>
    Task<IReadOnlyList<string>> ReserveSlugsAsync(IReadOnlyList<SlugCandidate> candidates, CancellationToken cancellationToken = default);

    /// <summary>Creates the franchise with a unique slug generated from its name.</summary>
    Task<Franchise> CreateAsync(CreateFranchiseCommand command, CancellationToken cancellationToken = default);
    Task UpdateBrandAsync(int id, UpdateFranchiseCommand command, CancellationToken cancellationToken = default);

    /// <summary>Deletes the franchise. Throws when any team still belongs to it.</summary>
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);

    Task UpdateLogoAsync(int id, string tempKey, CancellationToken cancellationToken = default);
    Task UpdateBannerAsync(int id, string tempKey, CancellationToken cancellationToken = default);

    /// <summary>Replaces the franchise's set of owners with the given accounts.</summary>
    Task SetOwnersAsync(int id, IReadOnlyCollection<int> accountIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Moves every team of <paramref name="sourceId"/> into <paramref name="targetId"/>, adds the source's owners
    /// to the target and deletes the source. The target keeps its branding and slug.
    /// </summary>
    Task MergeAsync(int sourceId, int targetId, CancellationToken cancellationToken = default);

    /// <summary>Moves one team to another franchise. The old franchise stays, even when left empty.</summary>
    Task MoveTeamAsync(int teamId, int targetFranchiseId, CancellationToken cancellationToken = default);
}