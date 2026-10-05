namespace BoltonCup.Core;

/// <summary>A collection of photos from the tournament, published on the site.</summary>
public class Album : EntityBase, ITaggable<AlbumTag>
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    /// <summary>URL segment, unique and lowercase. Generated from the title when blank.</summary>
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    /// <summary>Photo credit, shown on the album page.</summary>
    public string? Source { get; set; }
    public DateTime OccurredAt { get; set; }
    public bool IsPublished { get; set; }
    public DateTime? PublishedAt { get; set; }
    /// <summary>Picked from the album's own images; falls back to the first image when unset.</summary>
    public int? CoverImageId { get; set; }
    public AlbumImage? CoverImage { get; set; }

    public ICollection<AlbumImage> Images { get; set; } = [];
    public ICollection<AlbumTag> Tags { get; set; } = [];

    public override string ToString() => string.IsNullOrWhiteSpace(Title) ? $"Album {Id}" : Title;
}

public class AlbumComparer : IEqualityComparer<Album>
{
    public bool Equals(Album? item1, Album? item2)
    {
        if (ReferenceEquals(item1, item2))
        {
            return true;
        }

        return item1 is not null && item2 is not null && item1.Id == item2.Id;
    }

    public int GetHashCode(Album item) => item.Id;
}