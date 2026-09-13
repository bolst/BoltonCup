namespace BoltonCup.Core;

public interface ITagService<TTag>
    where TTag : EntityTag
{
    /// <summary>Tags a subject with one target. Returns the persisted tag, whose Id the caller needs to remove it later.</summary>
    Task<TTag> AddTagAsync(int subjectId, TagTargetType type, int targetId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Tags a subject with a free-text label, creating the label if it does not already exist.
    /// Label and tag are written together, so a failure cannot leave an unused label behind.
    /// </summary>
    Task<TTag> AddLabelTagAsync(int subjectId, string labelName, CancellationToken cancellationToken = default);

    Task RemoveTagAsync(int tagId, CancellationToken cancellationToken = default);
}
