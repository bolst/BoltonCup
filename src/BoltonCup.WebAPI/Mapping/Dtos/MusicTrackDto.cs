namespace BoltonCup.WebAPI.Mapping;

/// <summary>DTO representing a single music track that can be selected as a song request.</summary>
public record MusicTrackDto
{
    /// <summary>Gets the provider-specific track identifier.</summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>Gets the track title.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Gets the primary artist name.</summary>
    public string Artist { get; init; } = string.Empty;

    /// <summary>Gets the album art thumbnail URL, if available.</summary>
    public string? AlbumArtUrl { get; init; }
}
