using Microsoft.AspNetCore.DataProtection;

namespace BoltonCup.Shared.Data;

public class CustomUserService
{

    private readonly ICustomLocalStorageProvider _customLocalStorageProvider;

    public CustomUserService(ICustomLocalStorageProvider customLocalStorageProvider)
    {
        _customLocalStorageProvider = customLocalStorageProvider; 
    }

    public async Task PersistSessionToBrowserAsync(Supabase.Gotrue.Session session)
    {
        try
        {
            if (string.IsNullOrEmpty(session.AccessToken) || string.IsNullOrEmpty(session.RefreshToken))
            {
                Console.WriteLine($"Session provided no access/refresh token");
                return;
            }
            
            await _customLocalStorageProvider.SetAsync("access", session.AccessToken);
            await _customLocalStorageProvider.SetAsync("refresh", session.RefreshToken);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Failed to persist user to browser with:\n\t{e}");
        }
    }

    public async Task<(string, string)> FetchTokensFromBrowserAsync()
    {
        try
        {
            var accessToken = await _customLocalStorageProvider.GetAsync<string>("access");
            var refreshToken = await _customLocalStorageProvider.GetAsync<string>("refresh");

            if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(refreshToken))
            {
                Console.WriteLine("No tokens found");
                return (string.Empty, string.Empty);
            }

            return (accessToken, refreshToken);
        }
        catch (System.InvalidOperationException) { }
        catch (Exception err)
        {
            Console.WriteLine($"Failed to load session with:\n\t{err}");
        }
        return (string.Empty, string.Empty);
    }

    public async Task ClearBrowserStorageAsync()
    {
        await _customLocalStorageProvider.ClearAsync();
    }
}