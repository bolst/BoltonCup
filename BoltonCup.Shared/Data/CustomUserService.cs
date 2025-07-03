using Microsoft.AspNetCore.DataProtection;

namespace BoltonCup.Shared.Data;

public class CustomUserService
{

    private readonly ICustomLocalStorageProvider _customLocalStorageProvider;
    private readonly IBCData _bcData;
    private readonly Supabase.Client _supabaseClient;
    private readonly IDataProtector _protector;

    public CustomUserService(ICustomLocalStorageProvider customLocalStorageProvider, IBCData bcData, Supabase.Client supabaseClient)
    {
        _customLocalStorageProvider = customLocalStorageProvider; 
        _bcData = bcData;
        _supabaseClient = supabaseClient;
        _protector = DataProtectionProvider.Create("BC_PROTECTION").CreateProtector("tokenz");
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

            var protectedAccess = _protector.Protect(session.AccessToken);
            var protectedRefresh = _protector.Protect(session.RefreshToken);
            
            await _customLocalStorageProvider.SetAsync("access", protectedAccess);
            await _customLocalStorageProvider.SetAsync("refresh", protectedRefresh);
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
            var protectedAccess = await _customLocalStorageProvider.GetAsync<string>("access");
            var protectedRefresh = await _customLocalStorageProvider.GetAsync<string>("refresh");
            
            if (string.IsNullOrEmpty(protectedAccess) || string.IsNullOrEmpty(protectedRefresh))
            {
                Console.WriteLine("No tokens found");
                return (string.Empty, string.Empty);
            }
            
            var accessToken = _protector.Unprotect(protectedAccess);
            var refreshToken = _protector.Unprotect(protectedRefresh);

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