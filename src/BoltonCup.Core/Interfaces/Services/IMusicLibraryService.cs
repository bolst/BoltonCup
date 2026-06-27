using BoltonCup.Core.Commands;

namespace BoltonCup.Core;

public interface IMusicLibraryService
{
    /// <summary>All music tracks uploaded for a tournament.</summary>
    Task<IReadOnlyList<TournamentMusicTrack>> GetLibraryAsync(int tournamentId, CancellationToken cancellationToken = default);

    /// <summary>Commits an uploaded temp file to its final location and persists a new library track.</summary>
    Task<TournamentMusicTrack> AddTrackAsync(AddMusicTrackCommand command, CancellationToken cancellationToken = default);

    /// <summary>
    /// Ensures a track exists for the given (tournament, provider, track id), returning its id. An existing row
    /// (player request, import, or upload) has its metadata refreshed; a new row is inserted as a pending,
    /// non-base-pool request so the fetcher downloads it. Used to register team goal/win song selections.
    /// </summary>
    Task<int> EnsureTrackAsync(int tournamentId, MusicTrack track, MusicTrackSource sourceIfNew, CancellationToken cancellationToken = default);

    /// <summary>Updates a track's metadata / base-pool flag.</summary>
    Task UpdateTrackAsync(UpdateMusicTrackCommand command, CancellationToken cancellationToken = default);

    /// <summary>Removes a library track (DB row only; the R2 object is left in place).</summary>
    Task DeleteTrackAsync(int tournamentId, int trackId, CancellationToken cancellationToken = default);

    /// <summary>
    /// The ordered, de-duped playlist for a game (matched player requests first, then base pool) plus the
    /// player requests that have no matching uploaded file.
    /// </summary>
    Task<GamePlaylistResult> GetGamePlaylistAsync(int gameId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Tournament-wide download queue items still pending (no matching library file yet). Backs the
    /// homelab fetcher's "missing" list. Player requests are reconciled into the queue on read.
    /// </summary>
    Task<IReadOnlyList<MissingSongRequest>> GetMissingRequestsAsync(int tournamentId, CancellationToken cancellationToken = default);

    /// <summary>The full download queue (pending, downloaded, and cancelled) after reconciling player requests.</summary>
    Task<IReadOnlyList<MusicQueueItemView>> GetQueueAsync(int tournamentId, CancellationToken cancellationToken = default);

    /// <summary>Adds a public Spotify playlist's tracks to the queue as base-pool items. Returns the count added or reactivated.</summary>
    Task<int> ImportPlaylistAsync(int tournamentId, string playlistUrlOrId, CancellationToken cancellationToken = default);

    /// <summary>Soft-deletes a queue item (sets it cancelled); player request rows on their entity are untouched.</summary>
    Task CancelQueueItemAsync(int tournamentId, int queueItemId, CancellationToken cancellationToken = default);
}

public record GamePlaylistResult(IReadOnlyList<TournamentMusicTrack> Tracks, IReadOnlyList<MissingSongRequest> Missing);

public record MusicQueueItemView(
    int Id,
    string? TrackId,
    string? Title,
    string? Artist,
    string? AlbumArtUrl,
    MusicTrackStatus Status,
    MusicTrackSource Source,
    bool IsInBasePool,
    string? RequestedByName);

public record MissingSongRequest(string PlayerName, string? SongName, string? SongTrackId, bool IsInBasePool);
