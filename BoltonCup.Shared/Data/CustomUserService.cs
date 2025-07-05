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
    
    public async Task PersistSessionAsync(Supabase.Gotrue.Session session)
    {
        try
        {
            if (string.IsNullOrEmpty(session.AccessToken) || string.IsNullOrEmpty(session.RefreshToken))
            {
                // Console.WriteLine($"Session provided no access/refresh token");
                return;
            }
            
            var localIdStr = await _customLocalStorageProvider.GetAsync<string>("local_id");
            // if local storage has no value or is improper guid: we generate a new one for local storage
            // use this to lookup refresh token in db
            if (localIdStr is null || !Guid.TryParse(localIdStr, out Guid localId))
            {
                localId = Guid.NewGuid();
                await _customLocalStorageProvider.SetAsync("local_id", localId.ToString());
            }

            await _bcData.UpdateRefreshTokenAsync(new BCRefreshToken
            {
                refresh = session.RefreshToken,
                access = session.AccessToken,
                local_id = localId,
                provider = TokenProvider.Supabase.ToDescriptionString(),
            });
        }
        catch (Exception e)
        {
            // Console.WriteLine($"Failed to persist user to browser with:\n\t{e}");
        }
    }

    public async Task<(string?, string?)> FetchPersistedTokensAsync()
    {
        try
        {
            var localIdStr = await _customLocalStorageProvider.GetAsync<string>("local_id");
            // if local storage has no value or is improper guid: we generate a new one for local storage
            // use this to lookup refresh token in db
            if (localIdStr is null || !Guid.TryParse(localIdStr, out Guid localId))
            {
                localId = Guid.NewGuid();
                await _customLocalStorageProvider.SetAsync("local_id", localId.ToString());
            }

            var token = await _bcData.GetRefreshTokenAsync(localId, TokenProvider.Supabase);

            return (token?.access, token?.refresh);
        }
        catch (System.InvalidOperationException) { }
        catch (Exception err)
        {
            // Console.WriteLine($"Failed to load session with:\n\t{err}");
        }
        return (null, null);
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