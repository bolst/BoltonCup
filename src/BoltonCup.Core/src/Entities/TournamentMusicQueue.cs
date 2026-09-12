namespace BoltonCup.Core;

/// <summary>
/// The shared, non-repeating music rotation for a tournament, persisted so it survives page reloads and API
/// restarts. One row per tournament. All int values reference <see cref="TournamentMusicTrack.Id"/>.
/// </summary>
/// <remarks>
/// The order served to a game is <c>Priority ++ Deck[DeckCursor..]</c>. Element 0 is always the
/// currently-playing track, so a reload resumes it. The client reports each track it starts, which advances
/// the cursor; when it plays past the end of the deck the client asks the server to reshuffle a new cycle.
/// </remarks>
public class TournamentMusicQueue : EntityBase
{
    public int Id { get; set; }
    public required int TournamentId { get; set; }

    /// <summary>Shuffled base-pool track ids for the current cycle.</summary>
    public List<int> Deck { get; set; } = [];

    /// <summary>Index into <see cref="Deck"/> of the current base-pool song; entries before it are played this cycle.</summary>
    public int DeckCursor { get; set; }

    /// <summary>Injected player-song track ids for the current game, still pending (play-next lane).</summary>
    public List<int> Priority { get; set; } = [];

    /// <summary>The last-reported currently-playing track, used as the resume anchor. Null before first play.</summary>
    public int? CurrentTrackId { get; set; }

    public Tournament Tournament { get; set; } = null!;
}