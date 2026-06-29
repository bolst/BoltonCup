namespace BoltonCup.Core.Commands;

public sealed record UpdateMusicTrackCommand(
    int TournamentId,
    int Id,
    string Title,
    string? Artist,
    string? TrackId,
    MusicProviderType ProviderType,
    string? AlbumArtUrl,
    int? DurationMs,
    int? OffsetSeconds,
    bool IsInBasePool
);
