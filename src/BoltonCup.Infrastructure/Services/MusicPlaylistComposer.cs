using BoltonCup.Core;

namespace BoltonCup.Infrastructure.Services;

/// <summary>
/// Pure, side-effect-free composition of a game's playlist from player song requests and a tournament's
/// music library. Kept separate so the matching + ordering + de-dupe rules are unit-testable.
/// </summary>
public static class MusicPlaylistComposer
{
    /// <summary>
    /// Builds the ordered, de-duped track list for a game: player requests (in the given order) matched to
    /// library files by provider + track id first, then the tournament's base-pool tracks. Duplicates (by
    /// audio file key) are dropped, keeping the first occurrence. Tracks whose id is in
    /// <paramref name="excludeTrackIds"/> (e.g. team goal/win songs) are never included, even if requested
    /// or flagged for the base pool — they are only ever played as one-shots on goal/win.
    /// </summary>
    public static List<TournamentMusicTrack> Compose(
        IEnumerable<(MusicProviderType Provider, string? TrackId)> requests,
        IReadOnlyCollection<TournamentMusicTrack> library,
        IReadOnlySet<int>? excludeTrackIds = null)
    {
        var byKey = library
            .Where(t => !string.IsNullOrWhiteSpace(t.TrackId))
            .GroupBy(t => Key(t.ProviderType, t.TrackId!), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var result = new List<TournamentMusicTrack>();
        bool IsExcluded(TournamentMusicTrack t) => excludeTrackIds is not null && excludeTrackIds.Contains(t.Id);

        foreach (var (provider, trackId) in requests)
        {
            if (!string.IsNullOrWhiteSpace(trackId)
                && byKey.TryGetValue(Key(provider, trackId), out var track)
                && !IsExcluded(track)
                && seen.Add(track.AudioFileKey))
            {
                result.Add(track);
            }
        }

        foreach (var track in library.Where(t => t.IsInBasePool))
        {
            if (!IsExcluded(track) && seen.Add(track.AudioFileKey))
            {
                result.Add(track);
            }
        }

        return result;
    }

    static string Key(MusicProviderType provider, string trackId) => $"{provider}:{trackId}";
}