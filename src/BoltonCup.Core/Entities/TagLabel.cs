namespace BoltonCup.Core;

/// <summary>A free-text label usable as a tag target, e.g. "OT winner" or "fight". Global across tournaments.</summary>
public class TagLabel : EntityBase
{
    public int Id { get; set; }
    public required string Name { get; set; }

    public override string ToString() => Name;
}

public class TagLabelComparer : IEqualityComparer<TagLabel>
{
    public bool Equals(TagLabel? item1, TagLabel? item2)
    {
        if (ReferenceEquals(item1, item2))
        {
            return true;
        }

        return item1 is not null && item2 is not null && item1.Id == item2.Id;
    }

    public int GetHashCode(TagLabel item) => item.Id;
}
