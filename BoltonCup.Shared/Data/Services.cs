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


public class SupabaseServiceProvider
{
    private readonly Supabase.Client _supabase;
    private readonly EmailContentBuilder _emailContentBuilder;
    private readonly IBCData _bcData;

    public SupabaseServiceProvider(Supabase.Client supabase, IBCData bcData)
    {
        _supabase = supabase;
        _bcData = bcData;
        _emailContentBuilder = new EmailContentBuilder();
    }

    public async Task SendRegistrationEmail(RegisterFormModel form)
    {
        var tournament = await _bcData.GetCurrentTournamentAsync();
        var content = _emailContentBuilder.BuildRegistrationEmail(form, tournament);
        await SendEmail(form.Email, "Registration - Bolton Cup 2025", content);
    }

    public async Task SendPaymentConfirmationEmail(RegisterFormModel form)
    {
        var tournament = await _bcData.GetCurrentTournamentAsync();
        var content = _emailContentBuilder.BuildPaymentConfirmationEmail(form, tournament);
    }
    
    

    private async Task SendEmail(string to, string subject, string html)
    {
        var options = new Supabase.Functions.Client.InvokeFunctionOptions
        {
            Body = new Dictionary<string, object>
            {
                { "name", "Functions" },
                { "to", to },
                { "subject", subject },
                { "html", html }
            }
        };
        await _supabase.Functions.Invoke("resend-email", options: options);
    }
    
    
    
    
    
    private class EmailContentBuilder
    {
        public string BuildRegistrationEmail(RegisterFormModel form, BCTournament? tournament)
        {
            var paymentBody = string.Empty;
            
            if (tournament is not null && tournament.payment_open)
            {
                paymentBody =
                    $"""
                     <p>To complete your registration, you must pay the $150 fee at the link below. Spots are limited. Make sure to use the same email ({form.Email})!</p>
                                         <p><a href="{tournament.payment_link}">{tournament.payment_link}</a></p>
                     """;
            }
            
            var content = $"""
                           <p><img src="https://iiedqecnfyojvubvugmy.supabase.co/storage/v1/object/public/logos//bc-new.png" style="height: 200px; width: 200px;"></p>
                                               <h3 id="registration-complete">Registration Complete</h3>
                                               <p>Hi {form.FirstName},</p>
                                               <p>You have signed up for BOLTON CUP 2025.</p>
                                               <ul>
                                               <li>Name: {form.FirstName} {form.LastName}</li>
                                               <li>Position: {form.Position}</li>
                                               <li>Highest Level: {form.HighestLevel}</li>
                                               </ul>
                                               {paymentBody}
                                               <h3 id="have-questions-">Have Questions?</h3>
                                               <ul>
                                               <li>Nic Bolton: nicbolton17@icloud.com</li>
                                               </ul>
                           """;
            return content;

        }

        public string BuildPaymentConfirmationEmail(RegisterFormModel form, BCTournament? tournament)
        {
            var content = string.Empty;
            return content;
        }
    }
}

