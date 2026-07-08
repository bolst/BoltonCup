using BoltonCup.Core;
using BoltonCup.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BoltonCup.Infrastructure.Services;

/// <summary>
/// DB-backed implementation of <see cref="IGlobalMusicQueue"/>. Loads the per-tournament row, runs the pure
/// <see cref="MusicQueueEngine"/>, and saves. Registered explicitly as scoped (its name ends in
/// <c>Queue</c>, so the convention registrar leaves it alone).
/// </summary>
public class GlobalMusicQueue : IGlobalMusicQueue
{
    private readonly BoltonCupDbContext _db;
    private readonly Random _rng;

    public GlobalMusicQueue(BoltonCupDbContext db) : this(db, Random.Shared) { }

    // Test seam: inject a seeded Random for deterministic shuffles.
    public GlobalMusicQueue(BoltonCupDbContext db, Random rng)
    {
        _db = db;
        _rng = rng;
    }

    public async Task<IReadOnlyList<int>> GetOrderAsync(int tournamentId, CancellationToken cancellationToken = default)
    {
        var queue = await LoadOrCreateAsync(tournamentId, cancellationToken);
        var eligible = await GetEligibleBasePoolIdsAsync(tournamentId, cancellationToken);
        MusicQueueEngine.Reconcile(queue, eligible, _rng);
        await _db.SaveChangesAsync(cancellationToken);
        return MusicQueueEngine.BuildOrder(queue);
    }

    public async Task AdvanceAsync(int tournamentId, int trackId, CancellationToken cancellationToken = default)
    {
        var queue = await _db.TournamentMusicQueues.FirstOrDefaultAsync(q => q.TournamentId == tournamentId, cancellationToken);
        if (queue is null)
            return; // nothing to advance yet

        var eligible = await GetEligibleBasePoolIdsAsync(tournamentId, cancellationToken);
        MusicQueueEngine.Reconcile(queue, eligible, _rng);
        MusicQueueEngine.Advance(queue, trackId);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task RollOverAsync(int tournamentId, CancellationToken cancellationToken = default)
    {
        var queue = await LoadOrCreateAsync(tournamentId, cancellationToken);
        var eligible = await GetEligibleBasePoolIdsAsync(tournamentId, cancellationToken);
        MusicQueueEngine.RollOver(queue, eligible, _rng);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task StartGameAsync(int tournamentId, IReadOnlyList<int> playerTrackIds, CancellationToken cancellationToken = default)
    {
        var queue = await LoadOrCreateAsync(tournamentId, cancellationToken);
        var eligible = await GetEligibleBasePoolIdsAsync(tournamentId, cancellationToken);
        MusicQueueEngine.Reconcile(queue, eligible, _rng);
        // Player songs must be downloaded and not a team goal/win song to play in the rotation lane.
        var excluded = await GetExcludedTrackIdsAsync(tournamentId, cancellationToken);
        var downloaded = await GetDownloadedTrackIdsAsync(tournamentId, cancellationToken);
        var ids = playerTrackIds.Where(id => downloaded.Contains(id) && !excluded.Contains(id)).ToList();
        MusicQueueEngine.EnqueueNext(queue, ids);
        await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task<TournamentMusicQueue> LoadOrCreateAsync(int tournamentId, CancellationToken cancellationToken)
    {
        var queue = await _db.TournamentMusicQueues.FirstOrDefaultAsync(q => q.TournamentId == tournamentId, cancellationToken);
        if (queue is null)
        {
            queue = new TournamentMusicQueue { TournamentId = tournamentId };
            _db.TournamentMusicQueues.Add(queue);
        }
        return queue;
    }

    // Base-pool tracks that are downloaded and not reserved as a team goal/win song.
    private async Task<IReadOnlyList<int>> GetEligibleBasePoolIdsAsync(int tournamentId, CancellationToken cancellationToken)
    {
        var excluded = await GetExcludedTrackIdsAsync(tournamentId, cancellationToken);
        return await _db.TournamentMusicTracks
            .Where(t => t.TournamentId == tournamentId
                && t.Status == MusicTrackStatus.Downloaded
                && t.IsInBasePool
                && !excluded.Contains(t.Id))
            .OrderBy(t => t.Id)
            .Select(t => t.Id)
            .ToListAsync(cancellationToken);
    }

    private async Task<HashSet<int>> GetDownloadedTrackIdsAsync(int tournamentId, CancellationToken cancellationToken)
    {
        return (await _db.TournamentMusicTracks
            .Where(t => t.TournamentId == tournamentId && t.Status == MusicTrackStatus.Downloaded)
            .Select(t => t.Id)
            .ToListAsync(cancellationToken))
            .ToHashSet();
    }

    private async Task<HashSet<int>> GetExcludedTrackIdsAsync(int tournamentId, CancellationToken cancellationToken)
    {
        return (await _db.Teams
            .Where(t => t.TournamentId == tournamentId)
            .Select(t => new { t.GoalSongTrackId, t.WinSongTrackId })
            .ToListAsync(cancellationToken))
            .SelectMany(t => new[] { t.GoalSongTrackId, t.WinSongTrackId })
            .Where(id => id is not null)
            .Select(id => id!.Value)
            .ToHashSet();
    }
}
