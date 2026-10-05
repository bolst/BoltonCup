namespace BoltonCup.WebAPI.Mapping;

/// <summary>DTO representing one photo in an album.</summary>
public record AlbumImageDto
{
    /// <summary>Gets the unique identifier of the image.</summary>
    public required int Id { get; init; }
    /// <summary>Gets the absolute URL of the image.</summary>
    public required string Url { get; init; }
    /// <summary>Gets the tags on the image: labels, teams, tournaments, games or accounts.</summary>
    public required IReadOnlyList<TagDto> Tags { get; init; }
}