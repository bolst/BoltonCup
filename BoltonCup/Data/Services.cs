namespace BoltonCup.Data;

using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using Supabase;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

public interface ICacheService
{
    Task<T> GetOrAddAsync<T>(string cacheKey, Func<Task<T>> factory, TimeSpan cacheDuration);
    void Clear();
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



}

public class BCAuth
{
    private readonly string url;
    private readonly string key;
    private readonly Supabase.Client client;
    public BCAuth(string _url, string _key)
    {
        url = _url;
        key = _key;
        var options = new SupabaseOptions
        {
            AutoRefreshToken = true,
            AutoConnectRealtime = true,
            // SessionHandler = new SupabaseSessionHandler() <-- This must be implemented by the developer
        };
        client = new Supabase.Client(url, key, options);
    }
}