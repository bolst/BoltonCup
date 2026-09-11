namespace BoltonCup.Core;

public class Highlight : EntityBase, ITaggable<HighlightTag>
{
    public int Id { get; set; }
    public string? VideoId { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }

    public ICollection<HighlightTag> Tags { get; set; } = [];

    public override string ToString() => Title
        ?? VideoId
        ?? $"Highlight {Id}";
}

public class HighlightComparer : IEqualityComparer<Highlight>
{
    public bool Equals(Highlight? item1, Highlight? item2)
    {
        if (ReferenceEquals(item1, item2))
        {
            return true;
        }

        return item1 is not null && item2 is not null && item1.Id == item2.Id;
    }

    public int GetHashCode(Highlight item) => item.Id;
}
