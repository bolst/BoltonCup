namespace BoltonCup.Core.Commands;

public sealed record AddMusicTrackCommand(
    int TournamentId,
    string TempKey,
    string Title,
    string? Artist,
    string? TrackId,
    MusicProviderType ProviderType,
    string? AlbumArtUrl,
    int? DurationMs,
    bool IsInBasePool
);
