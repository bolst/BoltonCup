namespace BoltonCup.Core;

/// <summary>A post (0 when unsaved) and the slug it would like to use.</summary>
public sealed record SlugCandidate(int PostId, string Slug);

public interface INewsPostService
{
    /// <summary>Published posts, newest first.</summary>
    Task<IPagedList<NewsPost>> GetPublishedAsync(GetNewsPostsQuery query, CancellationToken cancellationToken = default);

    /// <summary>The published post with this exact slug, or null.</summary>
    Task<NewsPost?> GetPublishedBySlugAsync(string slug, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns one unique slug per candidate, in order. A candidate keeps its slug when no other
    /// post owns it; otherwise a numeric suffix is appended. Candidates are unique among themselves too.
    /// </summary>
    Task<IReadOnlyList<string>> ReserveSlugsAsync(IReadOnlyList<SlugCandidate> candidates, CancellationToken cancellationToken = default);

    /// <summary>Moves an uploaded temp asset into place as the post's cover image and stores its key.</summary>
    Task UpdateCoverImageAsync(int id, string tempKey, CancellationToken cancellationToken = default);
}
