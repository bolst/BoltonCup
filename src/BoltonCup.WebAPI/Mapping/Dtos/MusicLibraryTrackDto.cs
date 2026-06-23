using BoltonCup.Core;

namespace BoltonCup.WebAPI.Mapping;

/// <summary>A track in a tournament's uploaded music library.</summary>
public record MusicLibraryTrackDto
{
    /// <summary>Gets the track id.</summary>
    public int Id { get; init; }

    /// <summary>Gets the tournament this track belongs to.</summary>
    public int TournamentId { get; init; }

    /// <summary>Gets the R2 object key for the uploaded audio file.</summary>
    public string FileKey { get; init; } = string.Empty;

    /// <summary>Gets the provider-specific track id used to match player requests, if tagged.</summary>
    public string? TrackId { get; init; }

    /// <summary>Gets which provider <see cref="TrackId"/> belongs to.</summary>
    public MusicProviderType ProviderType { get; init; }

    /// <summary>Gets the track title.</summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>Gets the artist name, if known.</summary>
    public string? Artist { get; init; }

    /// <summary>Gets the album art URL, if known.</summary>
    public string? AlbumArtUrl { get; init; }

    /// <summary>Gets the track duration in milliseconds, if known.</summary>
    public int? DurationMs { get; init; }

    /// <summary>Gets whether this track plays in every game as part of the base pool.</summary>
    public bool IsInBasePool { get; init; }
}
