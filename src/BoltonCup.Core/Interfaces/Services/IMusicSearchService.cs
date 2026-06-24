namespace BoltonCup.Core;

public interface IMusicSearchService
{
    Task<IReadOnlyList<MusicTrack>> SearchTracksAsync(string query, int limit, CancellationToken cancellationToken = default);

    /// <summary>Tracks of a public Spotify playlist. Accepts a playlist URL, URI, or raw id.</summary>
    Task<IReadOnlyList<MusicTrack>> GetPlaylistTracksAsync(string playlistUrlOrId, CancellationToken cancellationToken = default);
}

public record MusicTrack(string Id, string Name, string Artist, string? AlbumArtUrl);
