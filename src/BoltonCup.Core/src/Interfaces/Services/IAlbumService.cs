namespace BoltonCup.Core;

public interface IAlbumService
{
    /// <summary>Published albums, newest first.</summary>
    Task<IPagedList<Album>> GetPublishedAsync(GetAlbumsQuery query, CancellationToken cancellationToken = default);

    /// <summary>The published album with this exact slug, with its images sorted by SortOrder, or null.</summary>
    Task<Album?> GetPublishedBySlugAsync(string slug, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns one unique slug per candidate, in order. A candidate keeps its slug when no other
    /// album owns it; otherwise a numeric suffix is appended. Candidates are unique among themselves too.
    /// </summary>
    Task<IReadOnlyList<string>> ReserveSlugsAsync(IReadOnlyList<SlugCandidate> candidates, CancellationToken cancellationToken = default);

    /// <summary>Stores the content as a new image on the album and returns the created row.</summary>
    Task<AlbumImage> AddImageAsync(int albumId, Stream content, string extension, string contentType, CancellationToken cancellationToken = default);

    /// <summary>Deletes the image row. The underlying R2 object is not deleted.</summary>
    Task DeleteImageAsync(int imageId, CancellationToken cancellationToken = default);

    /// <summary>Sets the album's cover image. The image must belong to the album.</summary>
    Task SetCoverImageAsync(int albumId, int? imageId, CancellationToken cancellationToken = default);

    /// <summary>Persists a new display order for the album's images.</summary>
    Task ReorderImagesAsync(int albumId, IReadOnlyList<int> orderedImageIds, CancellationToken cancellationToken = default);

    /// <summary>Copies each album tag onto every image that does not already carry it. Returns the number of tags added.</summary>
    Task<int> ApplyAlbumTagsToImagesAsync(int albumId, CancellationToken cancellationToken = default);

    /// <summary>Images in published albums carrying the given tag, newest album first.</summary>
    Task<IPagedList<AlbumImage>> GetPublishedImagesByTagAsync(GetAlbumImagesQuery query, CancellationToken cancellationToken = default);

    /// <summary>
    /// Copies the image to a new temp key and returns it. Returns null when the image does not
    /// exist or its album is not published.
    /// </summary>
    Task<string?> StagePublishedImageAsync(int imageId, CancellationToken cancellationToken = default);
}