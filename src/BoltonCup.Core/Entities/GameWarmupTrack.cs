namespace BoltonCup.Core;

/// <summary>
/// A track assigned to a game's warmup playlist, ordered by <see cref="Position"/>. Warmup tracks play at
/// the start of a game (before the shared tournament rotation) and are game-specific, not part of the pool.
/// </summary>
public class GameWarmupTrack : EntityBase
{
    public int Id { get; set; }
    public required int GameId { get; set; }
    public required int TournamentMusicTrackId { get; set; }

    /// <summary>Zero-based playback order within the game's warmup list.</summary>
    public int Position { get; set; }

    public Game Game { get; set; } = null!;
    public TournamentMusicTrack Track { get; set; } = null!;
}
