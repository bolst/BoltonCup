using BoltonCup.Sdk;

namespace BoltonCup.Timekeeper.Services.Music;

/// <summary>
/// Owns a music preload (offline download) run and its progress state, independent of any UI.
/// The download loop is a fire-and-forget async chain: in single-threaded Blazor WASM each
/// <see cref="MusicCacheService.DownloadAsync"/> awaits a JS fetch and yields to the UI between
/// tracks, so closing the dialog just removes an observer — the run keeps going in the background.
/// </summary>
public sealed class MusicDownloadService
{
    readonly MusicCacheService _cache;
    readonly SyncService _sync;

    CancellationTokenSource? _cts;
    readonly Dictionary<string, ItemStatus> _statuses = new();

    public MusicDownloadService(MusicCacheService cache, SyncService sync)
    {
        _cache = cache;
        _sync = sync;
    }

    public enum ItemStatus { Pending, Downloading, Done, Skipped, Failed }

    public bool IsRunning { get; private set; }
    public bool IsOffline { get; private set; }
    public int Total { get; private set; }
    public int Done { get; private set; }
    public int Skipped { get; private set; }
    public int Failed { get; private set; }
    public string? StorageText { get; private set; }

    /// <summary>Per-track status keyed by file key. Empty until a run has started.</summary>
    public IReadOnlyDictionary<string, ItemStatus> Statuses => _statuses;

    /// <summary>Downloaded + skipped + failed out of total, 0–100.</summary>
    public int OverallPercent => Total == 0 ? 100 : (int)(100.0 * (Done + Skipped + Failed) / Total);

    public event Action? OnProgressChanged;

    /// <summary>
    /// Starts a preload run for <paramref name="tracks"/>. No-op if a run is already in progress.
    /// Returns immediately; progress is reported via <see cref="OnProgressChanged"/>. When
    /// <paramref name="normalize"/> is true, each track's loudness is measured and stored on download
    /// (and back-filled for already-cached tracks) so playback can be level-normalized.
    /// </summary>
    public void Start(IReadOnlyList<PlaylistTrackDto> tracks, bool normalize)
    {
        if (IsRunning)
        {
            return;
        }

        IsOffline = false;
        StorageText = null;

        if (!_sync.IsOnline)
        {
            IsOffline = true;
            Notify();
            return;
        }

        _normalize = normalize;
        _cts = new CancellationTokenSource();
        _ = RunAsync(tracks, _cts.Token);
    }

    /// <summary>Cancels the in-progress run, if any. Already-downloaded tracks stay cached.</summary>
    public void Cancel() => _cts?.Cancel();

    /// <summary>Re-runs only the tracks that previously failed.</summary>
    public void Retry()
    {
        if (IsRunning)
        {
            return;
        }

        var failed = _statuses.Where(kv => kv.Value == ItemStatus.Failed).Select(kv => kv.Key).ToArray();
        if (failed.Length == 0)
        {
            return;
        }

        var tracks = _lastTracks.Where(t => failed.Contains(t.FileKey)).ToList();
        if (tracks.Count == 0)
        {
            return;
        }

        _cts = new CancellationTokenSource();
        _ = RunAsync(tracks, _cts.Token);
    }

    IReadOnlyList<PlaylistTrackDto> _lastTracks = [];
    bool _normalize;

    async Task RunAsync(IReadOnlyList<PlaylistTrackDto> tracks, CancellationToken ct)
    {
        IsRunning = true;
        IsOffline = false;
        _lastTracks = tracks;
        Total = tracks.Count;
        Done = 0;
        Skipped = 0;
        Failed = 0;
        StorageText = null;
        foreach (var track in tracks)
        {
            _statuses[track.FileKey] = ItemStatus.Pending;
        }
        Notify();

        try
        {
            await _cache.RequestPersistAsync();
            var cached = await _cache.GetCachedKeysAsync(tracks.Select(t => t.FileKey));

            foreach (var track in tracks)
            {
                if (ct.IsCancellationRequested)
                {
                    break;
                }

                if (cached.Contains(track.FileKey))
                {
                    // Already downloaded — still back-fill a gain measurement if it's missing.
                    if (_normalize)
                    {
                        await _cache.EnsureGainAsync(track.FileKey);
                    }
                    _statuses[track.FileKey] = ItemStatus.Skipped;
                    Skipped++;
                    Notify();
                    continue;
                }

                _statuses[track.FileKey] = ItemStatus.Downloading;
                Notify();

                var ok = await _cache.DownloadAsync(track.FileKey, _normalize);
                if (ok)
                {
                    _statuses[track.FileKey] = ItemStatus.Done;
                    Done++;
                }
                else
                {
                    _statuses[track.FileKey] = ItemStatus.Failed;
                    Failed++;
                }
                Notify();
            }

            var estimate = await _cache.GetStorageEstimateAsync();
            if (estimate.Quota > 0)
            {
                StorageText = $"Using {Megabytes(estimate.Usage)} of {Megabytes(estimate.Quota)} available.";
            }
        }
        catch (Exception)
        {
            // Fire-and-forget: never let the discarded task throw unobserved. Any tracks left
            // mid-flight remain Pending/Downloading; the operator can retry.
        }
        finally
        {
            IsRunning = false;
            Notify();
        }
    }

    static string Megabytes(long bytes) => $"{bytes / 1024.0 / 1024.0:0} MB";

    void Notify() => OnProgressChanged?.Invoke();
}