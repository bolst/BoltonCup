namespace BoltonCup.Core;

public interface IMusicSearchService
{
    Task<IReadOnlyList<MusicTrack>> SearchTracksAsync(string query, int limit, CancellationToken cancellationToken = default);
}

public record MusicTrack(string Id, string Name, string Artist, string? AlbumArtUrl);
