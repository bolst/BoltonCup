namespace BoltonCup.Core;

/// <summary>Lifecycle of a <see cref="TournamentMusicTrack"/>.</summary>
public enum MusicTrackStatus
{
    /// <summary>Wanted but not yet downloaded (no audio file); the fetcher will fetch it.</summary>
    Pending,

    /// <summary>Has an audio file and is part of the playable library.</summary>
    Downloaded,

    /// <summary>An admin removed this from the queue; sync never re-adds it.</summary>
    Cancelled,
}
