namespace BoltonCup.Core;

/// <summary>A Markdown news post. Global (not tournament-scoped); tags carry any team, game, tournament, account or label context.</summary>
public class NewsPost : EntityBase, ITaggable<NewsPostTag>
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    /// <summary>URL segment, unique and lowercase. Generated from the title when blank.</summary>
    public string Slug { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string? MarkdownContent { get; set; }
    /// <summary>R2 asset key of the cover image; resolved to a URL at the API boundary.</summary>
    public string? CoverImage { get; set; }
    public bool IsPublished { get; set; }
    public DateTime? PublishedAt { get; set; }

    public ICollection<NewsPostTag> Tags { get; set; } = [];

    public override string ToString() => string.IsNullOrWhiteSpace(Title) ? $"News post {Id}" : Title;
}

public class NewsPostComparer : IEqualityComparer<NewsPost>
{
    public bool Equals(NewsPost? item1, NewsPost? item2)
    {
        if (ReferenceEquals(item1, item2))
        {
            return true;
        }

        return item1 is not null && item2 is not null && item1.Id == item2.Id;
    }

    public int GetHashCode(NewsPost item) => item.Id;
}
