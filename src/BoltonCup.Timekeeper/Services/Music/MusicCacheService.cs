using BoltonCup.Core;
using Microsoft.JSInterop;

namespace BoltonCup.Timekeeper.Services.Music;

/// <summary>
/// Stores and retrieves game audio in IndexedDB (via musicCache.js), keyed by R2 file key. The base pool is
/// shared across games, so caching by key means it is only ever downloaded once.
/// </summary>
public sealed class MusicCacheService : IAsyncDisposable
{
    private readonly IJSRuntime _js;
    private readonly IAssetUrlResolver _urlResolver;
    private readonly SyncService _sync;
    private IJSObjectReference? _module;

    public MusicCacheService(IJSRuntime js, IAssetUrlResolver urlResolver, SyncService sync)
    {
        _js = js;
        _urlResolver = urlResolver;
        _sync = sync;
    }

    private async Task<IJSObjectReference> ModuleAsync()
        // Query string dodges Blazor's fingerprint import map (see MusicPlayerService) so the dev server serves it.
        => _module ??= await _js.InvokeAsync<IJSObjectReference>("import", "/js/musicCache.js?v=1");

    public async Task<bool> IsCachedAsync(string fileKey)
        => await (await ModuleAsync()).InvokeAsync<bool>("has", fileKey);

    public async Task<IReadOnlySet<string>> GetCachedKeysAsync(IEnumerable<string> fileKeys)
    {
        var keys = fileKeys.ToArray();
        var module = await ModuleAsync();
        var cached = await module.InvokeAsync<string[]>("hasMany", (object)keys);
        return cached.ToHashSet();
    }

    public async Task<bool> DownloadAsync(string fileKey)
    {
        var url = _urlResolver.GetFullUrl(fileKey);
        if (url is null) return false;
        var module = await ModuleAsync();
        var result = await module.InvokeAsync<DownloadResult>("download", fileKey, url);
        return result.Ok;
    }

    /// <summary>Cached → blob object URL (offline-capable); else online → R2 URL; else null (cannot play).</summary>
    public async Task<string?> GetPlayableUrlAsync(string fileKey)
    {
        var module = await ModuleAsync();
        var objectUrl = await module.InvokeAsync<string?>("getObjectUrl", fileKey);
        if (!string.IsNullOrEmpty(objectUrl))
            return objectUrl;
        return _sync.IsOnline ? _urlResolver.GetFullUrl(fileKey) : null;
    }

    public async Task RevokeAsync(string objectUrl)
        => await (await ModuleAsync()).InvokeVoidAsync("revokeObjectUrl", objectUrl);

    public async Task<bool> RequestPersistAsync()
        => await (await ModuleAsync()).InvokeAsync<bool>("requestPersist");

    public async Task<StorageEstimate> GetStorageEstimateAsync()
        => await (await ModuleAsync()).InvokeAsync<StorageEstimate>("estimate");

    public async ValueTask DisposeAsync()
    {
        if (_module is not null)
        {
            try { await _module.DisposeAsync(); } catch (JSDisconnectedException) { }
        }
    }

    public sealed class DownloadResult
    {
        public bool Ok { get; set; }
        public int Status { get; set; }
        public long Size { get; set; }
    }

    public sealed class StorageEstimate
    {
        public long Usage { get; set; }
        public long Quota { get; set; }
    }
}
