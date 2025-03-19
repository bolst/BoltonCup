using System.Diagnostics;
using Microsoft.AspNetCore.DataProtection;

namespace BoltonCup.Shared.Data;

public class CustomUserService
{

    private readonly ICustomLocalStorageProvider _customLocalStorageProvider;
    private readonly IBCData _bcData;
    private readonly Supabase.Client _supabaseClient;

    public CustomUserService(ICustomLocalStorageProvider customLocalStorageProvider, IBCData bcData, Supabase.Client supabaseClient)
    {
        _customLocalStorageProvider = customLocalStorageProvider; 
        _bcData = bcData;
        _supabaseClient = supabaseClient;
    }

    public async Task<BCAccount?> LookupAccountAsync(string email)
    {
        var account = await _bcData.GetAccountByEmailAsync(email);
        return account;
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

    public async Task<string> UpdateProfilePictureAsync(string email, byte[] imageBytes)
    {
        try
        {
            var account = await _bcData.GetAccountByEmailAsync(email);
            if (account is null) return "Could not find an account with that email";

            var filename = $"{account.FirstName}-{account.LastName}.png";
            var options = new Supabase.Storage.FileOptions
            {
                ContentType = "data:image/*;base64",
                Upsert = true,
            };

            await _supabaseClient.Storage.From("profile-pictures").Upload(imageBytes, filename, options);
            var publicUrl = _supabaseClient.Storage.From("profile-pictures").GetPublicUrl(filename);
            await _bcData.UpdateAccountProfilePictureAsync(account.Email, publicUrl);
        }
        catch (Supabase.Storage.Exceptions.SupabaseStorageException exc)
        {
            return "File is too large!";
        }
        catch (Exception e)
        {
            return "Something went wrong";
        }
        
        return string.Empty;
    }
    
}