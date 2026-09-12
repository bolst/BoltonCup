namespace BoltonCup.Core;

/// <summary>An entity that can carry tags. Implement to make a new subject type taggable.</summary>
public interface ITaggable<TTag>
    where TTag : EntityTag
{
    int Id { get; }
    ICollection<TTag> Tags { get; set; }
}
