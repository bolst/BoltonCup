namespace BoltonCup.WebAPI.Mapping;

/// <summary>Detailed DTO for a single album, including its photos.</summary>
public record AlbumSingleDto : AlbumDto
{
    /// <summary>Gets the album description.</summary>
    public string? Description { get; init; }
    /// <summary>Gets the photo credit.</summary>
    public string? Source { get; init; }
    /// <summary>Gets the album's photos, ordered by their display order.</summary>
    public required IReadOnlyList<AlbumImageDto> Images { get; init; }
}