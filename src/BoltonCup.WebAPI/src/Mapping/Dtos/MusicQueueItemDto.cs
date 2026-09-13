using BoltonCup.Core;

namespace BoltonCup.WebAPI.Mapping;

/// <summary>An item in a tournament's music download queue.</summary>
public record MusicQueueItemDto
{
    /// <summary>Gets the queue item id.</summary>
    public int Id { get; init; }

    /// <summary>Gets the Spotify track id (the match/dedupe key).</summary>
    public string? TrackId { get; init; }

    /// <summary>Gets the song title, if known.</summary>
    public string? Title { get; init; }

    /// <summary>Gets the artist name, if known.</summary>
    public string? Artist { get; init; }

    /// <summary>Gets the album art URL, if known.</summary>
    public string? AlbumArtUrl { get; init; }

    /// <summary>Gets the download status.</summary>
    public MusicTrackStatus Status { get; init; }

    /// <summary>Gets where this item originated.</summary>
    public MusicTrackSource Source { get; init; }

    /// <summary>Gets whether the downloaded file becomes a base-pool track.</summary>
    public bool IsInBasePool { get; init; }

    /// <summary>Gets the requesting player's name, for player-sourced items.</summary>
    public string? RequestedByName { get; init; }
}