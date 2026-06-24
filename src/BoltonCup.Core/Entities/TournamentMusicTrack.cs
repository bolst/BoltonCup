namespace BoltonCup.Core;

/// <summary>
/// A song the tournament wants in its music library, tracked through its lifecycle. A row may be
/// <see cref="MusicTrackStatus.Pending"/> (wanted, no file yet), <see cref="MusicTrackStatus.Downloaded"/>
/// (has an audio file, playable), or <see cref="MusicTrackStatus.Cancelled"/>. Player song requests are
/// reconciled into this table; admins add uploads and playlist imports directly.
/// </summary>
public class TournamentMusicTrack : EntityBase
{
    public int Id { get; set; }
    public required int TournamentId { get; set; }

    /// <summary>Object key for the uploaded audio file. Null until the track is downloaded.</summary>
    public string? AudioFileKey { get; set; }

    /// <summary>Provider-specific track id used to match player song requests. Null for untagged uploads.</summary>
    public string? TrackId { get; set; }

    /// <summary>Which provider <see cref="TrackId"/> belongs to.</summary>
    public MusicProviderType ProviderType { get; set; } = MusicProviderType.Spotify;

    /// <summary>Song title. May be null for a pending request whose name is unknown.</summary>
    public string? Title { get; set; }
    public string? Artist { get; set; }
    public string? AlbumArtUrl { get; set; }
    public int? DurationMs { get; set; }

    /// <summary>When true, this track plays in every game of the tournament as part of the base pool.</summary>
    public bool IsInBasePool { get; set; } = true;

    /// <summary>Where this row stands in its download lifecycle.</summary>
    public MusicTrackStatus Status { get; set; } = MusicTrackStatus.Downloaded;

    /// <summary>How this row was created.</summary>
    public MusicTrackSource Source { get; set; } = MusicTrackSource.ManualUpload;

    /// <summary>Display name of a player who requested this song (null for uploads/imports).</summary>
    public string? RequestedByName { get; set; }

    public Tournament Tournament { get; set; } = null!;

    public override string ToString() => Artist is null ? Title ?? string.Empty : $"{Title} — {Artist}";
}

public class TournamentMusicTrackComparer : IEqualityComparer<TournamentMusicTrack>
{
    public bool Equals(TournamentMusicTrack? item1, TournamentMusicTrack? item2)
    {
        if (ReferenceEquals(item1, item2))
            return true;
        return item1 is not null && item2 is not null && item1.Id == item2.Id;
    }

    public int GetHashCode(TournamentMusicTrack item) => item.Id;
}
