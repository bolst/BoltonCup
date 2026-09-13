using BoltonCup.Core;
using BoltonCup.Core.Commands;

namespace BoltonCup.WebAPI.Mapping;

public partial class Mapper
{
    // ---------- Music ----------

    public IReadOnlyList<MusicTrackDto> ToDto(IReadOnlyList<MusicTrack> tracks) => tracks.Select(track => new MusicTrackDto
    {
        Id = track.Id,
        Name = track.Name,
        Artist = track.Artist,
        AlbumArtUrl = track.AlbumArtUrl,
    }).ToList();

    public GamePlaylistDto ToDto(GamePlaylistResult result) => new GamePlaylistDto
    {
        Warmup = result.Warmup.Select(ToPlaylistTrackDto).ToList(),
        Tracks = result.Tracks.Select(ToPlaylistTrackDto).ToList(),
        Missing = result.Missing.Select(ToMissingSongRequestDto).ToList(),
    };

    static PlaylistTrackDto ToPlaylistTrackDto(TournamentMusicTrack t) => new PlaylistTrackDto
    {
        MusicTrackId = t.Id,
        FileKey = t.AudioFileKey,
        Title = t.Title,
        Artist = t.Artist,
        AlbumArtUrl = t.AlbumArtUrl,
        DurationMs = t.DurationMs,
        OffsetSeconds = t.OffsetSeconds,
        RequestedByName = t.RequestedByName,
    };

    public IReadOnlyList<MusicLibraryTrackDto> ToDtoList(IReadOnlyList<TournamentMusicTrack> tracks)
        => tracks.Select(ToDto).ToList();

    public MusicLibraryTrackDto ToDto(TournamentMusicTrack track) => new MusicLibraryTrackDto
    {
        Id = track.Id,
        TournamentId = track.TournamentId,
        FileKey = track.AudioFileKey ?? string.Empty,
        TrackId = track.TrackId,
        ProviderType = track.ProviderType,
        Title = track.Title,
        Artist = track.Artist,
        AlbumArtUrl = track.AlbumArtUrl,
        DurationMs = track.DurationMs,
        OffsetSeconds = track.OffsetSeconds,
        IsInBasePool = track.IsInBasePool,
    };

    public IReadOnlyList<MissingSongRequestDto> ToDtoList(IReadOnlyList<MissingSongRequest> missing)
        => missing.Select(ToMissingSongRequestDto).ToList();

    static MissingSongRequestDto ToMissingSongRequestDto(MissingSongRequest m) => new MissingSongRequestDto
    {
        PlayerName = m.PlayerName,
        SongName = m.SongName,
        SongTrackId = m.SongTrackId,
        IsInBasePool = m.IsInBasePool,
    };

    public IReadOnlyList<MusicQueueItemDto> ToDtoList(IReadOnlyList<MusicQueueItemView> queue)
        => queue.Select(ToMusicQueueItemDto).ToList();

    static MusicQueueItemDto ToMusicQueueItemDto(MusicQueueItemView q) => new MusicQueueItemDto
    {
        Id = q.Id,
        TrackId = q.TrackId,
        Title = q.Title,
        Artist = q.Artist,
        AlbumArtUrl = q.AlbumArtUrl,
        Status = q.Status,
        Source = q.Source,
        IsInBasePool = q.IsInBasePool,
        RequestedByName = q.RequestedByName,
    };

    public AddMusicTrackCommand ToCommand(int tournamentId, AddMusicTrackRequest request)
        => new AddMusicTrackCommand(tournamentId,
            request.TempKey,
            request.Title,
            request.Artist,
            request.TrackId,
            request.ProviderType,
            request.AlbumArtUrl,
            request.DurationMs,
            null,
            request.IsInBasePool);

    public UpdateMusicTrackCommand ToCommand(int tournamentId, int trackId, UpdateMusicTrackRequest request)
        => new UpdateMusicTrackCommand(tournamentId,
            trackId,
            request.Title,
            request.Artist,
            request.TrackId,
            request.ProviderType,
            request.AlbumArtUrl,
            request.DurationMs,
            null,
            request.IsInBasePool);
}
