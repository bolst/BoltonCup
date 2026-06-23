using BoltonCup.Core.Commands;

namespace BoltonCup.Core;

public interface IMusicLibraryService
{
    /// <summary>All music tracks uploaded for a tournament.</summary>
    Task<IReadOnlyList<TournamentMusicTrack>> GetLibraryAsync(int tournamentId, CancellationToken cancellationToken = default);

    /// <summary>Commits an uploaded temp file to its final location and persists a new library track.</summary>
    Task<TournamentMusicTrack> AddTrackAsync(AddMusicTrackCommand command, CancellationToken cancellationToken = default);

    /// <summary>Updates a track's metadata / base-pool flag.</summary>
    Task UpdateTrackAsync(UpdateMusicTrackCommand command, CancellationToken cancellationToken = default);

    /// <summary>Removes a library track (DB row only; the R2 object is left in place).</summary>
    Task DeleteTrackAsync(int tournamentId, int trackId, CancellationToken cancellationToken = default);

    /// <summary>
    /// The ordered, de-duped playlist for a game (matched player requests first, then base pool) plus the
    /// player requests that have no matching uploaded file.
    /// </summary>
    Task<GamePlaylistResult> GetGamePlaylistAsync(int gameId, CancellationToken cancellationToken = default);

    /// <summary>Tournament-wide player song requests that have no matching uploaded file yet.</summary>
    Task<IReadOnlyList<MissingSongRequest>> GetMissingRequestsAsync(int tournamentId, CancellationToken cancellationToken = default);
}

public record GamePlaylistResult(IReadOnlyList<TournamentMusicTrack> Tracks, IReadOnlyList<MissingSongRequest> Missing);

public record MissingSongRequest(string PlayerName, string? SongName, string? SongTrackId);
