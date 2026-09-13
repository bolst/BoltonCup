using BoltonCup.Core;

namespace BoltonCup.WebAPI.Mapping;

/// <summary>Request to add an uploaded audio file to a tournament's music library.</summary>
public record AddMusicTrackRequest
{
    /// <summary>Gets or sets the temp upload key returned by the assets upload flow.</summary>
    public string TempKey { get; set; } = string.Empty;

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