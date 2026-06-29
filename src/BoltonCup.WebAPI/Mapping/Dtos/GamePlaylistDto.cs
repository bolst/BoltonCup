namespace BoltonCup.WebAPI.Mapping;

/// <summary>The computed music playlist for a game: ordered playable tracks plus unplayable (missing) requests.</summary>
public record GamePlaylistDto
{
    /// <summary>Gets the ordered, de-duped tracks: matched player requests first, then base pool.</summary>
    public IReadOnlyList<PlaylistTrackDto> Tracks { get; init; } = [];

    /// <summary>Gets the player song requests that have no matching uploaded file.</summary>
    public IReadOnlyList<MissingSongRequestDto> Missing { get; init; } = [];
}
/// <summary>A single playable track in a game playlist.</summary>
public record PlaylistTrackDto
{
    /// <summary>Gets the R2 object key the client resolves and caches for playback.</summary>
    public string FileKey { get; init; } = string.Empty;

    /// <summary>Gets the track title.</summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>Gets the artist name, if known.</summary>
    public string? Artist { get; init; }

    /// <summary>Gets the album art URL, if known.</summary>
    public string? AlbumArtUrl { get; init; }

    /// <summary>Gets the track duration in milliseconds, if known.</summary>
    public int? DurationMs { get; init; }
}
/// <summary>A player song request that could not be matched to an uploaded file.</summary>
public record MissingSongRequestDto
{
    /// <summary>Gets the requesting player's name.</summary>
    public string PlayerName { get; init; } = string.Empty;

    /// <summary>Gets the requested song name, if known.</summary>
    public string? SongName { get; init; }

    /// <summary>Gets the requested Spotify track id, if known.</summary>
    public string? SongTrackId { get; init; }

    /// <summary>Gets whether the downloaded file should be registered as a base-pool track.</summary>
    public bool IsInBasePool { get; init; }
}