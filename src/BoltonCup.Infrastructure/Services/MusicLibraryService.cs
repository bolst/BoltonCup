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
    private readonly IMusicSearchService _search;

    public MusicLibraryService(BoltonCupDbContext db, IStorageService storage, IAssetKeyGenerator keyGenerator, IMusicSearchService search)
    {
        _db = db;
        _storage = storage;
        _keyGenerator = keyGenerator;
        _search = search;
    }

    public async Task<IReadOnlyList<TournamentMusicTrack>> GetLibraryAsync(int tournamentId, CancellationToken cancellationToken = default)
    {
        // The library is the playable catalog: downloaded rows only (pending/cancelled live in the queue view).
        return await _db.TournamentMusicTracks
            .Where(t => t.TournamentId == tournamentId && t.Status == MusicTrackStatus.Downloaded)
            .OrderBy(t => t.Title)
            .ToListAsync(cancellationToken);
    }

    public async Task<TournamentMusicTrack> AddTrackAsync(AddMusicTrackCommand command, CancellationToken cancellationToken = default)
    {
        _keyGenerator.ThrowIfNotValidTempKey(command.TempKey);

        var extension = Path.GetExtension(command.TempKey);
        var finalKey = _keyGenerator.GenerateFinalKey<TournamentMusicTrack>(command.TournamentId.ToString(), "audio", extension);
        await _storage.CopyAssetAsync(command.TempKey, finalKey, cancellationToken);

        // If a row already exists for this track id (a pending player request or playlist import), fill in
        // its file and graduate it to Downloaded rather than creating a duplicate. Untagged uploads always insert.
        TournamentMusicTrack? track = null;
        if (!string.IsNullOrWhiteSpace(command.TrackId))
        {
            track = await _db.TournamentMusicTracks.FirstOrDefaultAsync(
                t => t.TournamentId == command.TournamentId
                    && t.ProviderType == command.ProviderType
                    && t.TrackId == command.TrackId,
                cancellationToken);
        }

        if (track is null)
        {
            track = new TournamentMusicTrack
            {
                TournamentId = command.TournamentId,
                ProviderType = command.ProviderType,
                TrackId = command.TrackId,
                Source = MusicTrackSource.ManualUpload,
            };
            _db.TournamentMusicTracks.Add(track);
        }

        track.AudioFileKey = finalKey;
        track.Title = command.Title;
        track.Artist = command.Artist;
        track.AlbumArtUrl = command.AlbumArtUrl;
        track.DurationMs = command.DurationMs;
        track.OffsetSeconds = command.OffsetSeconds;
        track.IsInBasePool = command.IsInBasePool;
        track.Status = MusicTrackStatus.Downloaded;

        await _db.SaveChangesAsync(cancellationToken);
        return track;
    }

    public async Task<int> EnsureTrackAsync(int tournamentId, MusicTrack track, MusicTrackSource sourceIfNew, CancellationToken cancellationToken = default)
    {
        var existing = await _db.TournamentMusicTracks.FirstOrDefaultAsync(
            t => t.TournamentId == tournamentId
                && t.ProviderType == MusicProviderType.Spotify
                && t.TrackId == track.Id,
            cancellationToken);

        if (existing is null)
        {
            existing = new TournamentMusicTrack
            {
                TournamentId = tournamentId,
                ProviderType = MusicProviderType.Spotify,
                TrackId = track.Id,
                Status = MusicTrackStatus.Pending,
                Source = sourceIfNew,
                IsInBasePool = false,
            };
            _db.TournamentMusicTracks.Add(existing);
        }

        // Refresh display metadata whether the row is new or pre-existing.
        existing.Title = track.Name;
        existing.Artist = string.IsNullOrWhiteSpace(track.Artist) ? null : track.Artist;
        existing.AlbumArtUrl = track.AlbumArtUrl;

        await _db.SaveChangesAsync(cancellationToken);
        return existing.Id;
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
        track.OffsetSeconds = command.OffsetSeconds;
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
            .Where(t => t.TournamentId == game.TournamentId && t.Status == MusicTrackStatus.Downloaded)
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
            .Select(r => new MissingSongRequest(r.PlayerName, r.SongName, r.SongTrackId, false))
            .ToList();

        return new GamePlaylistResult(tracks, missing);
    }

    public async Task<IReadOnlyList<MissingSongRequest>> GetMissingRequestsAsync(int tournamentId, CancellationToken cancellationToken = default)
    {
        await SyncPlayerRequestsAsync(tournamentId, cancellationToken);

        return await _db.TournamentMusicTracks
            .Where(t => t.TournamentId == tournamentId && t.Status == MusicTrackStatus.Pending)
            .OrderBy(t => t.Title)
            .Select(t => new MissingSongRequest(t.RequestedByName ?? string.Empty, t.Title, t.TrackId, t.IsInBasePool))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<MusicQueueItemView>> GetQueueAsync(int tournamentId, CancellationToken cancellationToken = default)
    {
        await SyncPlayerRequestsAsync(tournamentId, cancellationToken);

        // The queue is everything not yet in the library: pending downloads and cancelled items.
        return await _db.TournamentMusicTracks
            .Where(t => t.TournamentId == tournamentId && t.Status != MusicTrackStatus.Downloaded)
            .OrderBy(t => t.Status == MusicTrackStatus.Pending ? 0 : 1) // pending first, then cancelled
            .ThenBy(t => t.Title)
            .Select(t => new MusicQueueItemView(
                t.Id, t.TrackId, t.Title, t.Artist, t.AlbumArtUrl, t.Status, t.Source, t.IsInBasePool, t.RequestedByName))
            .ToListAsync(cancellationToken);
    }

    public async Task<int> ImportPlaylistAsync(int tournamentId, string playlistUrlOrId, CancellationToken cancellationToken = default)
    {
        var tracks = await _search.GetPlaylistTracksAsync(playlistUrlOrId, cancellationToken);
        if (tracks.Count == 0)
            return 0;

        // Reconcile player requests first so imports dedupe against songs players already requested.
        await SyncPlayerRequestsAsync(tournamentId, cancellationToken);

        var existing = await _db.TournamentMusicTracks
            .Where(t => t.TournamentId == tournamentId && t.ProviderType == MusicProviderType.Spotify && t.TrackId != null)
            .ToListAsync(cancellationToken);
        var byTrackId = existing.ToDictionary(t => t.TrackId!, StringComparer.OrdinalIgnoreCase);

        var changed = 0;
        foreach (var track in tracks)
        {
            if (string.IsNullOrEmpty(track.Id))
                continue;

            if (byTrackId.TryGetValue(track.Id, out var item))
            {
                // Promote an existing row to base-pool; reactivate it if an admin had cancelled it.
                item.IsInBasePool = true;
                if (item.Status == MusicTrackStatus.Cancelled)
                {
                    item.Status = item.AudioFileKey is null ? MusicTrackStatus.Pending : MusicTrackStatus.Downloaded;
                    changed++;
                }
                continue;
            }

            var added = new TournamentMusicTrack
            {
                TournamentId = tournamentId,
                ProviderType = MusicProviderType.Spotify,
                TrackId = track.Id,
                Title = track.Name,
                Artist = string.IsNullOrWhiteSpace(track.Artist) ? null : track.Artist,
                AlbumArtUrl = track.AlbumArtUrl,
                Status = MusicTrackStatus.Pending,
                Source = MusicTrackSource.PlaylistImport,
                IsInBasePool = true,
            };
            _db.TournamentMusicTracks.Add(added);
            byTrackId[track.Id] = added;
            changed++;
        }

        await _db.SaveChangesAsync(cancellationToken);
        return changed;
    }

    public async Task CancelQueueItemAsync(int tournamentId, int queueItemId, CancellationToken cancellationToken = default)
    {
        var item = await _db.TournamentMusicTracks
            .FirstOrDefaultAsync(t => t.Id == queueItemId && t.TournamentId == tournamentId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(TournamentMusicTrack), queueItemId);

        item.Status = MusicTrackStatus.Cancelled;
        await _db.SaveChangesAsync(cancellationToken);
    }

    // Ensure every player song request has a row in the table. Insert-only: a downloaded row keeps its
    // file, and a cancelled row stays cancelled (an admin decision), so neither is disturbed here.
    private async Task SyncPlayerRequestsAsync(int tournamentId, CancellationToken cancellationToken)
    {
        var existingTrackIds = (await _db.TournamentMusicTracks
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
                i.SongArtist,
                i.SongAlbumArtUrl,
                i.SongTrackId,
            })
            .ToListAsync(cancellationToken);

        foreach (var r in requests)
        {
            if (!existingTrackIds.Add(r.SongTrackId!))
                continue;

            _db.TournamentMusicTracks.Add(new TournamentMusicTrack
            {
                TournamentId = tournamentId,
                ProviderType = MusicProviderType.Spotify,
                TrackId = r.SongTrackId!,
                Title = r.SongName,
                Artist = r.SongArtist,
                AlbumArtUrl = r.SongAlbumArtUrl,
                Status = MusicTrackStatus.Pending,
                Source = MusicTrackSource.PlayerRequest,
                IsInBasePool = false,
                RequestedByName = r.PlayerName.Trim(),
            });
        }

        await _db.SaveChangesAsync(cancellationToken);
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
