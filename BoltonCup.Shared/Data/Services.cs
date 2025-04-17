namespace BoltonCup.Shared.Data;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using Blazored.LocalStorage;

public interface ICacheService
{
    Task<T> GetOrAddAsync<T>(string cacheKey, Func<Task<T>> factory, TimeSpan cacheDuration);
    void Clear();
    void Clear(string cacheKey);
}
public class CacheService : ICacheService
{
    private readonly IMemoryCache memoryCache;
    private CancellationTokenSource resetCacheToken = new();
    public CacheService(IMemoryCache _memoryCache)
    {
        memoryCache = _memoryCache;
    }

    public async Task<T> GetOrAddAsync<T>(string cacheKey, Func<Task<T>> factory, TimeSpan cacheDuration)
    {
        if (!memoryCache.TryGetValue(cacheKey, out T? cacheEntry))
        {
            cacheEntry = await factory();

            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = cacheDuration,
            };
            cacheOptions.AddExpirationToken(new CancellationChangeToken(resetCacheToken.Token));

            memoryCache.Set(cacheKey, cacheEntry, cacheOptions);
        }

        return cacheEntry!;
    }

    public void Clear()
    {
        resetCacheToken.Cancel();
        resetCacheToken.Dispose();
        resetCacheToken = new CancellationTokenSource();
    }

    public void Clear(string cacheKey)
    {
        memoryCache.Remove(cacheKey);
    }
}

public class RegistrationStateService
{
    private readonly ICustomLocalStorageProvider _customLocalStorageProvider;

    public RegistrationStateService(ICustomLocalStorageProvider customLocalStorageService)
    {
        _customLocalStorageProvider = customLocalStorageService;
    }

    public async Task SetBrowserRegistered(bool state)
    {
        await _customLocalStorageProvider.SetAsync("reg", state);
    }

    public async Task<bool> GetBrowserRegistered()
    {
        return await _customLocalStorageProvider.GetAsync<bool>("reg");
    }
    
}