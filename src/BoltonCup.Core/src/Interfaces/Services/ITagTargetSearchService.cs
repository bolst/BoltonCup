namespace BoltonCup.Core;

public sealed record TagTargetOption(int Id, string Display);

public interface ITagTargetSearchService
{
    /// <summary>Finds taggable entities of one target type, excluding ids already tagged.</summary>
    Task<IReadOnlyList<TagTargetOption>> SearchAsync(
        TagTargetType type,
        string? term,
        IReadOnlyCollection<int> excludeIds,
        CancellationToken cancellationToken = default);
}
