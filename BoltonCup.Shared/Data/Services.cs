using Stripe;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;

namespace BoltonCup.Shared.Data;


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

public class StripeServiceProvider
{
    private readonly IBCData _bcData;
    
    public StripeServiceProvider(string apiKey, IBCData bcData)
    {
        StripeConfiguration.ApiKey = apiKey;
        _bcData = bcData;
    }

    public async Task<RegisterFormModel?> ProcessCheckoutAsync(string checkoutId)
    {
        try
        {
            var service = new Stripe.Checkout.SessionService();
            Stripe.Checkout.Session session = service.Get(checkoutId);

            var email = session.CustomerDetails.Email;
            if (string.IsNullOrEmpty(email))
            {
                return null;
            }

            var userData = await _bcData.GetRegistrationByEmailAsync(email);
            if (userData is null)
            {
                return null;
            }
            
            await _bcData.SetUserAsPayedAsync(email);

            return userData;
        }
        catch (Exception e)
        {
            return null;
        }
    }
    
}