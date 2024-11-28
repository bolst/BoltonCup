using System.Reflection.Metadata;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.DataProtection;
using Supabase.Gotrue;

namespace BoltonCup.Data;

public class CustomUserService
{
    private readonly IBCData BCData;
    private readonly Supabase.Client SBClient;
    private readonly NavigationManager Navigation;
    private readonly ILocalStorageService LocalStorage;
    private readonly IDataProtector DataProtector;

    public CustomUserService(Supabase.Client _SBClient, NavigationManager _Navigation, IBCData _BCData, ILocalStorageService _LocalStorage)
    {
        SBClient = _SBClient;
        Navigation = _Navigation;
        BCData = _BCData;
        LocalStorage = _LocalStorage;
        DataProtector = DataProtectionProvider
                            .Create("BOLTCUP")
                            .CreateProtector("creds");
    }

    public async Task<BCUser?> LookupUserInDatabase(string? email)
    {
        if (email is null) return null;
        BCUser? user = await BCData.GetUserByEmail(email);
        return user;
    }

    public async Task<bool> SignInWithGoogleAsync()
    {
        var state = await SBClient.Auth.SignIn(Constants.Provider.Google, new()
        {
            RedirectTo = $"{Navigation.BaseUri}auth/callback/googlesignin",
            FlowType = Constants.OAuthFlowType.PKCE,
        });

        if (state.PKCEVerifier is null) return false;

        await LocalStorage.SetItemAsStringAsync("verifier", DataProtector.Protect(state.PKCEVerifier));
        Navigation.NavigateTo(state.Uri.AbsoluteUri);
        
        return true;
    }

    public async Task<bool> VerifyOAuthCode(string code)
    {
        string? verifier = await LocalStorage.GetItemAsStringAsync("verifier");
        if (verifier is null) return false;
        
        verifier = DataProtector.Unprotect(await LocalStorage.GetItemAsStringAsync("verifier"));
        await LocalStorage.RemoveItemAsync("verifier");
        
        var session = await SBClient.Auth.ExchangeCodeForSession(verifier, code);
        Navigation.NavigateTo("/");
        
        return true;
    }

    public async Task<string?> UpdateProfilePicture(FileUpload file)
    {
        Supabase.Storage.FileOptions options = new()
        {
            ContentType = file.ContentType,
            Upsert = true,
        };

        string? userEmail = SBClient.Auth.CurrentUser?.Email;
        if (userEmail is null) return "Unauthorized";
        
        BCUser? user = await BCData.GetUserByEmail(userEmail);
        if (user is null) return "Could not find user";

        var profilePicture = await BCData.GetPlayerProfilePictureById(user.PlayerId);
        string supabaseImagePath = profilePicture.Source.Split('/').Last();
            
        await SBClient.Storage.From("profile-pictures").Update(file.Data, supabaseImagePath, options: options);
        return null;
    }
}