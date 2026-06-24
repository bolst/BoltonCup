namespace BoltonCup.Core;

/// <summary>Where a <see cref="TournamentMusicTrack"/> originated.</summary>
public enum MusicTrackSource
{
    /// <summary>Uploaded directly by an admin.</summary>
    ManualUpload,

    /// <summary>Reconciled from a player's song request on their <see cref="TournamentPlayerInfo"/>.</summary>
    PlayerRequest,

    /// <summary>Added by an admin importing a Spotify playlist.</summary>
    PlaylistImport,
}
