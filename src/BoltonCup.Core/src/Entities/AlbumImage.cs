namespace BoltonCup.Core;

/// <summary>One photo in an <see cref="Album"/>, stored in R2 under its own key.</summary>
public class AlbumImage : EntityBase, ITaggable<AlbumImageTag>
{
    public int Id { get; set; }
    public int AlbumId { get; set; }
    public Album Album { get; set; } = null!;
    /// <summary>R2 asset key; resolved to a URL at the API boundary.</summary>
    public string Key { get; set; } = string.Empty;
    public int SortOrder { get; set; }

    public ICollection<AlbumImageTag> Tags { get; set; } = [];

    public override string ToString() => $"Album image {Id}";
}

public class AlbumImageComparer : IEqualityComparer<AlbumImage>
{
    public bool Equals(AlbumImage? item1, AlbumImage? item2)
    {
        if (ReferenceEquals(item1, item2))
        {
            return true;
        }

        return item1 is not null && item2 is not null && item1.Id == item2.Id;
    }

    public int GetHashCode(AlbumImage item) => item.Id;
}