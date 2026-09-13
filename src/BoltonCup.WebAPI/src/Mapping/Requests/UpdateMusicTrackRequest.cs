using BoltonCup.Core;

namespace BoltonCup.WebAPI.Mapping;

/// <summary>Request to update a music library track's metadata or base-pool flag.</summary>
public record UpdateMusicTrackRequest
{
    /// <summary>Gets or sets the track title.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Gets or sets the artist name, if known.</summary>
    public string? Artist { get; set; }

    /// <summary>Gets or sets the provider-specific track id used to match player requests, if tagged.</summary>
    public string? TrackId { get; set; }

    /// <summary>Gets or sets which provider <see cref="TrackId"/> belongs to.</summary>
    public MusicProviderType ProviderType { get; set; } = MusicProviderType.Spotify;

    /// <summary>Gets or sets the album art URL, if known.</summary>
    public string? AlbumArtUrl { get; set; }

    /// <summary>Gets or sets the track duration in milliseconds, if known.</summary>
    public int? DurationMs { get; set; }

    /// <summary>Gets or sets whether this track plays in every game as part of the base pool.</summary>
    public bool IsInBasePool { get; set; } = true;
}