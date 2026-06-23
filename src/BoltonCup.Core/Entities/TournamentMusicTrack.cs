namespace BoltonCup.Core;

public class TournamentMusicTrack : EntityBase
{
    public int Id { get; set; }
    public required int TournamentId { get; set; }

    /// <summary>object key for the uploaded audio file.</summary>
    public required string AudioFileKey { get; set; }

    /// <summary>Provider-specific track id used to match player song requests to this file. Null until tagged.</summary>
    public string? TrackId { get; set; }

    /// <summary>Which provider <see cref="TrackId"/> belongs to.</summary>
    public MusicProviderType ProviderType { get; set; } = MusicProviderType.Spotify;

    public required string Title { get; set; }
    public string? Artist { get; set; }
    public string? AlbumArtUrl { get; set; }
    public int? DurationMs { get; set; }

    /// <summary>When true, this track plays in every game of the tournament as part of the base pool.</summary>
    public bool IsInBasePool { get; set; } = true;

    public Tournament Tournament { get; set; } = null!;

    public override string ToString() => Artist is null ? Title : $"{Title} — {Artist}";
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
