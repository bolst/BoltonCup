using BoltonCup.Core;
using BoltonCup.Core.Commands;
using BoltonCup.Core.Exceptions;
using BoltonCup.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BoltonCup.Infrastructure.Services;

public class MusicLibraryService : IMusicLibraryService
{
    private readonly BoltonCupDbContext _db;
    private readonly IStorageService _storage;
    private readonly IAssetKeyGenerator _keyGenerator;

    public MusicLibraryService(BoltonCupDbContext db, IStorageService storage, IAssetKeyGenerator keyGenerator)
    {
        _db = db;
        _storage = storage;
        _keyGenerator = keyGenerator;
    }

    public async Task<IReadOnlyList<TournamentMusicTrack>> GetLibraryAsync(int tournamentId, CancellationToken cancellationToken = default)
    {
        return await _db.TournamentMusicTracks
            .Where(t => t.TournamentId == tournamentId)
            .OrderBy(t => t.Title)
            .ToListAsync(cancellationToken);
    }

    public async Task<TournamentMusicTrack> AddTrackAsync(AddMusicTrackCommand command, CancellationToken cancellationToken = default)
    {
        _keyGenerator.ThrowIfNotValidTempKey(command.TempKey);

        var extension = Path.GetExtension(command.TempKey);
        var finalKey = _keyGenerator.GenerateFinalKey<TournamentMusicTrack>(command.TournamentId.ToString(), "audio", extension);
        await _storage.CopyAssetAsync(command.TempKey, finalKey, cancellationToken);

        var track = new TournamentMusicTrack
        {
            TournamentId = command.TournamentId,
            AudioFileKey = finalKey,
            Title = command.Title,
            Artist = command.Artist,
            TrackId = command.TrackId,
            ProviderType = command.ProviderType,
            AlbumArtUrl = command.AlbumArtUrl,
            DurationMs = command.DurationMs,
            IsInBasePool = command.IsInBasePool,
        };

        _db.TournamentMusicTracks.Add(track);
        await _db.SaveChangesAsync(cancellationToken);
        return track;
    }

    public async Task UpdateTrackAsync(UpdateMusicTrackCommand command, CancellationToken cancellationToken = default)
    {
        var track = await _db.TournamentMusicTracks
            .FirstOrDefaultAsync(t => t.Id == command.Id && t.TournamentId == command.TournamentId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(TournamentMusicTrack), command.Id);

        track.Title = command.Title;
        track.Artist = command.Artist;
        track.TrackId = command.TrackId;
        track.ProviderType = command.ProviderType;
        track.AlbumArtUrl = command.AlbumArtUrl;
        track.DurationMs = command.DurationMs;
        track.IsInBasePool = command.IsInBasePool;

        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteTrackAsync(int tournamentId, int trackId, CancellationToken cancellationToken = default)
    {
        var track = await _db.TournamentMusicTracks
            .FirstOrDefaultAsync(t => t.Id == trackId && t.TournamentId == tournamentId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(TournamentMusicTrack), trackId);

        _db.TournamentMusicTracks.Remove(track);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<GamePlaylistResult> GetGamePlaylistAsync(int gameId, CancellationToken cancellationToken = default)
    {
        var game = await _db.Games
            .FirstOrDefaultAsync(g => g.Id == gameId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Game), gameId);

        var requests = await GetOrderedRequestsAsync(game, cancellationToken);

        var library = await _db.TournamentMusicTracks
            .Where(t => t.TournamentId == game.TournamentId)
            .OrderBy(t => t.Title)
            .ToListAsync(cancellationToken);

        var tracks = MusicPlaylistComposer.Compose(
            requests.Select(r => (MusicProviderType.Spotify, r.SongTrackId)), library);

        var libraryTrackIds = library
            .Where(t => t.ProviderType == MusicProviderType.Spotify && !string.IsNullOrWhiteSpace(t.TrackId))
            .Select(t => t.TrackId!)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var missing = requests
            .Where(r => r.SongTrackId is null || !libraryTrackIds.Contains(r.SongTrackId))
            .Select(r => new MissingSongRequest(r.PlayerName, r.SongName, r.SongTrackId))
            .ToList();

        return new GamePlaylistResult(tracks, missing);
    }

    public async Task<IReadOnlyList<MissingSongRequest>> GetMissingRequestsAsync(int tournamentId, CancellationToken cancellationToken = default)
    {
        var libraryTrackIds = (await _db.TournamentMusicTracks
                .Where(t => t.TournamentId == tournamentId && t.ProviderType == MusicProviderType.Spotify && t.TrackId != null)
                .Select(t => t.TrackId!)
                .ToListAsync(cancellationToken))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var requests = await _db.TournamentPlayerInfos
            .Where(i => i.TournamentId == tournamentId && i.SongTrackId != null)
            .Select(i => new
            {
                PlayerName = i.Account.FirstName + " " + i.Account.LastName,
                i.SongName,
                i.SongTrackId,
            })
            .ToListAsync(cancellationToken);

        return requests
            .Where(r => !libraryTrackIds.Contains(r.SongTrackId!))
            .Select(r => new MissingSongRequest(r.PlayerName.Trim(), r.SongName, r.SongTrackId))
            .ToList();
    }

    // Player song requests for both teams in the game, home team first then away, in roster (jersey) order.
    private async Task<List<RequestRow>> GetOrderedRequestsAsync(Game game, CancellationToken cancellationToken)
    {
        var teamIds = new List<int>();
        if (game.HomeTeamId is { } home) teamIds.Add(home);
        if (game.AwayTeamId is { } away) teamIds.Add(away);
        if (teamIds.Count == 0)
            return [];

        var players = await _db.Players
            .Where(p => p.TournamentId == game.TournamentId && p.TeamId != null && teamIds.Contains(p.TeamId!.Value))
            .OrderBy(p => p.JerseyNumber ?? int.MaxValue)
            .Select(p => new { p.TeamId, p.AccountId, PlayerName = p.Account.FirstName + " " + p.Account.LastName })
            .ToListAsync(cancellationToken);

        if (players.Count == 0)
            return [];

        var accountIds = players.Select(p => p.AccountId).Distinct().ToList();

        var songByAccount = await _db.TournamentPlayerInfos
            .Where(i => i.TournamentId == game.TournamentId && accountIds.Contains(i.AccountId) && i.SongTrackId != null)
            .Select(i => new { i.AccountId, i.SongTrackId, i.SongName })
            .ToDictionaryAsync(i => i.AccountId, i => new { i.SongTrackId, i.SongName }, cancellationToken);

        var ordered = new List<RequestRow>();
        foreach(var player in players.Where(p => p.TeamId is {} teamId && teamIds.Contains(teamId)))
        {
            if (songByAccount.TryGetValue(player.AccountId, out var song))
            {
                ordered.Add(new RequestRow(player.PlayerName.Trim(), song.SongName, song.SongTrackId));
            }
        }

        return ordered;
    }

    private sealed record RequestRow(string PlayerName, string? SongName, string? SongTrackId);
}
