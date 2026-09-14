namespace BoltonCup.WebAPI.Mapping;

/// <summary>DTO representing a published news post in a list.</summary>
public record NewsPostDto
{
    /// <summary>Gets the unique identifier of the post.</summary>
    public required int Id { get; init; }
    /// <summary>Gets the title of the post.</summary>
    public required string Title { get; init; }
    /// <summary>Gets the URL slug of the post.</summary>
    public required string Slug { get; init; }
    /// <summary>Gets the short summary shown in lists.</summary>
    public string? Summary { get; init; }
    /// <summary>Gets the absolute URL of the cover image, if any.</summary>
    public string? CoverImageUrl { get; init; }
    /// <summary>Gets when the post was published (UTC).</summary>
    public DateTime? PublishedAt { get; init; }
    /// <summary>Gets the tags on the post: labels, teams, tournaments, games or accounts.</summary>
    public required IReadOnlyList<TagDto> Tags { get; init; }
}
