namespace BoltonCup.Core;

/// <summary>
/// The shared, non-repeating music rotation for a tournament, persisted across page reloads and API
/// restarts. Callers pass tournament ids; the queue owns the eligible base-pool set (downloaded base-pool
/// tracks minus team goal/win songs) and its own shuffled state.
/// </summary>
public interface IGlobalMusicQueue
{
    /// <summary>
    /// The ordered track ids for a tournament's playlist (current song first, then the rest of the cycle,
    /// then the upcoming cycle). Lazily creates and reconciles the deck against the current catalog.
    /// </summary>
    Task<IReadOnlyList<int>> GetOrderAsync(int tournamentId, CancellationToken cancellationToken = default);

    /// <summary>Records that a track is now playing, advancing the resume anchor.</summary>
    Task AdvanceAsync(int tournamentId, int trackId, CancellationToken cancellationToken = default);

    /// <summary>Starts a fresh shuffled cycle once the deck is exhausted (called when the client plays past the end).</summary>
    Task RollOverAsync(int tournamentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Injects a game's player songs at the front of the queue (replacing the previous game's) and resets the
    /// resume anchor so the fresh game starts at the front. Pass an empty list to just clear prior injections.
    /// </summary>
    Task StartGameAsync(int tournamentId, IReadOnlyList<int> playerTrackIds, CancellationToken cancellationToken = default);
}