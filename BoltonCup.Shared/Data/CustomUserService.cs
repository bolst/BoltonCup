using Microsoft.AspNetCore.DataProtection;
using Newtonsoft.Json;

namespace BoltonCup.Shared.Data;

public class CustomUserService
{

    private readonly ICustomLocalStorageProvider _customLocalStorageProvider;
    private readonly IDataProtector _dataProtector;

    public CustomUserService(ICustomLocalStorageProvider customLocalStorageProvider, IDataProtectionProvider dataProtectionProvider)
    {
        _customLocalStorageProvider = customLocalStorageProvider;
        _dataProtector = dataProtectionProvider.CreateProtector("creds");
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
            
            await _customLocalStorageProvider.SetAsync("access", _dataProtector.Protect(session.AccessToken));
            await _customLocalStorageProvider.SetAsync("refresh", _dataProtector.Protect(session.RefreshToken));
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

            accessToken = _dataProtector.Unprotect(accessToken);
            refreshToken = _dataProtector.Unprotect(refreshToken);

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