namespace BoltonCup.Core;

public sealed record TagTargetOption(int Id, string Display);

public interface ITagTargetSearchService
{
    /// <summary>
    /// Finds taggable entities of one target type, excluding ids already tagged. When
    /// <paramref name="tournamentId"/> is set, games and teams are limited to that tournament and
    /// accounts to its players; tournaments and labels are global and unaffected.
    /// </summary>
    Task<IReadOnlyList<TagTargetOption>> SearchAsync(
        TagTargetType type,
        string? term,
        IReadOnlyCollection<int> excludeIds,
        int? tournamentId = null,
        CancellationToken cancellationToken = default);
}
