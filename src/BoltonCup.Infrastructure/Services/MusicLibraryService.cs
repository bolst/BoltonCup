using BoltonCup.Core;
using BoltonCup.Core.Commands;
using BoltonCup.Core.Exceptions;
using BoltonCup.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BoltonCup.Infrastructure.Services;

public class MusicLibraryService : IMusicLibraryService
{
    readonly BoltonCupDbContext _db;
    readonly IStorageService _storage;
    readonly IAssetKeyGenerator _keyGenerator;
    readonly IMusicSearchService _search;
    readonly IGlobalMusicQueue _queue;

    public MusicLibraryService(BoltonCupDbContext db, IStorageService storage, IAssetKeyGenerator keyGenerator, IMusicSearchService search, IGlobalMusicQueue queue)
    {
        _db = db;
        _storage = storage;
        _keyGenerator = keyGenerator;
        _search = search;
        _queue = queue;
    }

    public async Task<IReadOnlyList<TournamentMusicTrack>> GetLibraryAsync(int tournamentId, CancellationToken cancellationToken = default) =>
        // The library is the playable catalog: downloaded rows only (pending/cancelled live in the queue view).
        await _db.TournamentMusicTracks
            .Where(t => t.TournamentId == tournamentId && t.Status == MusicTrackStatus.Downloaded)
            .OrderBy(t => t.Title)
            .ToListAsync(cancellationToken);

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
            .ToListAsync(cancellationToken);
        var byId = library.ToDictionary(t => t.Id);

        // Order comes from the shared tournament rotation (current song first). Map ids to downloaded tracks,
        // dropping any that vanished, and de-dupe by audio file key (keeping the first occurrence).
        var order = await _queue.GetOrderAsync(game.TournamentId, cancellationToken);
        var seenKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var tracks = new List<TournamentMusicTrack>();
        foreach (var id in order)
        {
            if (byId.TryGetValue(id, out var track)
                && !string.IsNullOrWhiteSpace(track.AudioFileKey)
                && seenKeys.Add(track.AudioFileKey!))
            {
                tracks.Add(track);
            }
        }

        var libraryTrackIds = library
            .Where(t => t.ProviderType == MusicProviderType.Spotify && !string.IsNullOrWhiteSpace(t.TrackId))
            .Select(t => t.TrackId!)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var missing = requests
            .Where(r => r.SongTrackId is null || !libraryTrackIds.Contains(r.SongTrackId))
            .Select(r => new MissingSongRequest(r.PlayerName, r.SongName, r.SongTrackId, false))
            .ToList();

        // Warmup plays first: game-specific tracks, ordered, filtered to downloaded rows with a file.
        var warmup = await GetGameWarmupInternalAsync(gameId, byId, cancellationToken);

        return new GamePlaylistResult(tracks, missing, warmup);
    }

    public async Task<IReadOnlyList<TournamentMusicTrack>> GetGameWarmupAsync(int gameId, CancellationToken cancellationToken = default)
        => await GetGameWarmupInternalAsync(gameId, byId: null, cancellationToken);

    // Ordered, downloaded warmup tracks for a game. Reuses a caller-supplied downloaded-track lookup when
    // available (the game-playlist path already has one) to avoid a second query.
    async Task<IReadOnlyList<TournamentMusicTrack>> GetGameWarmupInternalAsync(
        int gameId, IReadOnlyDictionary<int, TournamentMusicTrack>? byId, CancellationToken cancellationToken)
    {
        var warmupIds = await _db.GameWarmupTracks
            .Where(w => w.GameId == gameId)
            .OrderBy(w => w.Position)
            .Select(w => w.TournamentMusicTrackId)
            .ToListAsync(cancellationToken);
        if (warmupIds.Count == 0)
        {
            return [];
        }

        var lookup = byId;
        if (lookup is null)
        {
            lookup = await _db.TournamentMusicTracks
                .Where(t => warmupIds.Contains(t.Id) && t.Status == MusicTrackStatus.Downloaded)
                .ToDictionaryAsync(t => t.Id, cancellationToken);
        }

        var warmup = new List<TournamentMusicTrack>();
        foreach (var id in warmupIds)
        {
            if (lookup.TryGetValue(id, out var track) && !string.IsNullOrWhiteSpace(track.AudioFileKey))
            {
                warmup.Add(track);
            }
        }
        return warmup;
    }

    public async Task SetGameWarmupAsync(int gameId, IReadOnlyList<int> trackIds, CancellationToken cancellationToken = default)
    {
        var game = await _db.Games
            .Where(g => g.Id == gameId)
            .Select(g => new { g.TournamentId })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Game), gameId);

        // De-dupe while preserving order; validate every id belongs to the game's tournament.
        var orderedIds = trackIds.Distinct().ToList();
        if (orderedIds.Count > 0)
        {
            var validIds = await _db.TournamentMusicTracks
                .Where(t => t.TournamentId == game.TournamentId && orderedIds.Contains(t.Id))
                .Select(t => t.Id)
                .ToListAsync(cancellationToken);
            var invalid = orderedIds.Except(validIds).ToList();
            if (invalid.Count > 0)
            {
                throw new EntityNotFoundException(nameof(TournamentMusicTrack), invalid[0]);
            }
        }

        var existing = await _db.GameWarmupTracks
            .Where(w => w.GameId == gameId)
            .ToListAsync(cancellationToken);
        _db.GameWarmupTracks.RemoveRange(existing);

        for (var i = 0; i < orderedIds.Count; i++)
        {
            _db.GameWarmupTracks.Add(new GameWarmupTrack
            {
                GameId = gameId,
                TournamentMusicTrackId = orderedIds[i],
                Position = i,
            });
        }

        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task AdvanceGameQueueAsync(int gameId, int musicTrackId, CancellationToken cancellationToken = default)
    {
        var tournamentId = await GetTournamentIdAsync(gameId, cancellationToken);
        await _queue.AdvanceAsync(tournamentId, musicTrackId, cancellationToken);
    }

    public async Task RollGameQueueAsync(int gameId, CancellationToken cancellationToken = default)
    {
        var tournamentId = await GetTournamentIdAsync(gameId, cancellationToken);
        await _queue.RollOverAsync(tournamentId, cancellationToken);
    }

    public async Task<IReadOnlyList<int>> GetOrderedRequestTrackIdsAsync(int gameId, CancellationToken cancellationToken = default)
    {
        var game = await _db.Games.FirstOrDefaultAsync(g => g.Id == gameId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Game), gameId);

        var requests = await GetOrderedRequestsAsync(game, cancellationToken);
        var requestedTrackIds = requests
            .Where(r => !string.IsNullOrWhiteSpace(r.SongTrackId))
            .Select(r => r.SongTrackId!)
            .ToList();
        if (requestedTrackIds.Count == 0)
        {
            return [];
        }

        // Map each request's provider track id to its downloaded library row id, preserving roster order.
        var downloaded = await _db.TournamentMusicTracks
            .Where(t => t.TournamentId == game.TournamentId
                && t.Status == MusicTrackStatus.Downloaded
                && t.ProviderType == MusicProviderType.Spotify
                && t.TrackId != null)
            .Select(t => new { t.Id, t.TrackId })
            .ToListAsync(cancellationToken);
        var idByTrackId = downloaded
            .GroupBy(t => t.TrackId!, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First().Id, StringComparer.OrdinalIgnoreCase);

        var ids = new List<int>();
        foreach (var trackId in requestedTrackIds)
        {
            if (idByTrackId.TryGetValue(trackId, out var id) && !ids.Contains(id))
            {
                ids.Add(id);
            }
        }

        return ids;
    }

    async Task<int> GetTournamentIdAsync(int gameId, CancellationToken cancellationToken)
    {
        var game = await _db.Games
            .Where(g => g.Id == gameId)
            .Select(g => new { g.TournamentId })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Game), gameId);
        return game.TournamentId;
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
        {
            return 0;
        }

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
            {
                continue;
            }

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
    async Task SyncPlayerRequestsAsync(int tournamentId, CancellationToken cancellationToken)
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
            {
                continue;
            }

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
    async Task<List<RequestRow>> GetOrderedRequestsAsync(Game game, CancellationToken cancellationToken)
    {
        var teamIds = new List<int>();
        if (game.HomeTeamId is { } home)
        {
            teamIds.Add(home);
        }

        if (game.AwayTeamId is { } away)
        {
            teamIds.Add(away);
        }

        if (teamIds.Count == 0)
        {
            return [];
        }

        var players = await _db.Players
            .Where(p => p.TournamentId == game.TournamentId && p.TeamId != null && teamIds.Contains(p.TeamId!.Value))
            .OrderBy(p => p.JerseyNumber ?? int.MaxValue)
            .Select(p => new { p.TeamId, p.AccountId, PlayerName = p.Account.FirstName + " " + p.Account.LastName })
            .ToListAsync(cancellationToken);

        if (players.Count == 0)
        {
            return [];
        }

        var accountIds = players.Select(p => p.AccountId).Distinct().ToList();

        var songByAccount = await _db.TournamentPlayerInfos
            .Where(i => i.TournamentId == game.TournamentId && accountIds.Contains(i.AccountId) && i.SongTrackId != null)
            .Select(i => new { i.AccountId, i.SongTrackId, i.SongName })
            .ToDictionaryAsync(i => i.AccountId, i => new { i.SongTrackId, i.SongName }, cancellationToken);

        var ordered = new List<RequestRow>();
        foreach (var player in players.Where(p => p.TeamId is { } teamId && teamIds.Contains(teamId)))
        {
            if (songByAccount.TryGetValue(player.AccountId, out var song))
            {
                ordered.Add(new RequestRow(player.PlayerName.Trim(), song.SongName, song.SongTrackId));
            }
        }

        return ordered;
    }

    sealed record RequestRow(string PlayerName, string? SongName, string? SongTrackId);
}