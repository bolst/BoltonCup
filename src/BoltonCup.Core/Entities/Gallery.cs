namespace BoltonCup.Core;

public class Gallery : EntityBase
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Source { get; set; }

    public ICollection<Tournament> Tournaments { get; set; } = [];

    public override string ToString()
    {
        return string.IsNullOrEmpty(Title) 
            ? $"Gallery {Id}" 
            : Title;
    }
}


public class GalleryComparer : IEqualityComparer<Gallery>
{
    public bool Equals(Gallery? item1, Gallery? item2)
    {
        if (ReferenceEquals(item1, item2)) 
            return true;
        return item1 is not null && item2 is not null && item1.Id == item2.Id;
    }
        
    public int GetHashCode(Gallery item) => item.Id;
}