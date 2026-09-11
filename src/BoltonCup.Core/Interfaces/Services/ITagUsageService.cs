namespace BoltonCup.Core;

/// <summary>What a tag target is attached to, grouped by the kind of subject carrying it.</summary>
public sealed record TagUsageGroup(string SubjectName, IReadOnlyList<string> Items);

public interface ITagUsageService
{
    Task<IReadOnlyList<TagUsageGroup>> GetLabelUsageAsync(int labelId, CancellationToken cancellationToken = default);
}
