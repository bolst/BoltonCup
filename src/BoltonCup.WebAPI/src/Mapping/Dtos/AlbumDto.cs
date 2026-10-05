namespace BoltonCup.WebAPI.Mapping;

/// <summary>DTO representing a published album in a list.</summary>
public record AlbumDto
{
    /// <summary>Gets the unique identifier of the album.</summary>
    public required int Id { get; init; }
    /// <summary>Gets the title of the album.</summary>
    public required string Title { get; init; }
    /// <summary>Gets the URL slug of the album.</summary>
    public required string Slug { get; init; }
    /// <summary>Gets the absolute URL of the cover image, if any.</summary>
    public string? CoverImageUrl { get; init; }
    /// <summary>Gets when the photos were taken.</summary>
    public required DateTime OccurredAt { get; init; }
    /// <summary>Gets the tags on the album: labels, teams, tournaments, games or accounts.</summary>
    public required IReadOnlyList<TagDto> Tags { get; init; }
}