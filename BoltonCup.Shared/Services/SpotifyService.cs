using System.Globalization;
using SpotifyAPI.Web;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.DataProtection;
using Newtonsoft.Json;

namespace BoltonCup.Shared.Data;

public class SpotifyService
{

    private readonly string _clientId;
    private readonly string _secret;
    private readonly ILocalStorageService _localStorage;
    private readonly IDataProtector _protector;
    
    private readonly Uri LoginCallbackUri = new("https://127.0.0.1:7107/callback/");
    
    private ClientCredentialsTokenResponse? _clientToken;
    private AuthorizationCodeTokenResponse? _oauthToken;

    private readonly List<string> RequestScopes =
        [Scopes.UserReadPlaybackState, Scopes.UserModifyPlaybackState, Scopes.UserReadCurrentlyPlaying];

    private string? _oauthCode;


    public SpotifyService(string clientId, string secret, ILocalStorageService localStorage, IDataProtectionProvider provider)
    {
        _clientId = clientId;
        _secret = secret;
        _localStorage = localStorage;
        _protector = provider.CreateProtector("creds");
    }

    
    
    public Uri LoginRequestUri => new LoginRequest(LoginCallbackUri, _clientId, LoginRequest.ResponseType.Code)
    {
        Scope = RequestScopes,
    }.ToUri();

    public async Task<bool> GetAuthStateAsync()
    {
        var token = await GetAuthRequestToken();
        return token is not null && !token.IsExpired;
    }
    
    
    public async Task<SearchResponse> SearchSongs(string song, int limit = 10, CancellationToken cancellationToken = default)
    {
        try
        {
            var token = await GetClientRequestToken(cancellationToken);
            
            var spotify = new SpotifyClient(token);
            
            var request = new SearchRequest(SearchRequest.Types.Track, $"{song}");

            return await spotify.Search.Item(request, cancel: cancellationToken);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error in Spotify Search: {e.Message}");
            return new();
        }
    }


    public async Task<CurrentlyPlayingContext?> GetPlayerState(CancellationToken cancellationToken = default)
    {
        try
        {
            var token = await GetAuthRequestToken(cancellationToken);
            if (token is null) return null;

            SpotifyClientConfig.CreateDefault().WithToken(token.AccessToken);
            
            var spotify = new SpotifyClient(token);

            var request = new PlayerCurrentPlaybackRequest();
            
            return await spotify.Player.GetCurrentPlayback(request, cancel: cancellationToken);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return null;
        }

    }



    public async Task Pause(CancellationToken cancellationToken = default)
    {
        try
        {
            var token = await GetAuthRequestToken(cancellationToken);
            if (token is null) return;

            SpotifyClientConfig.CreateDefault().WithToken(token.AccessToken);
            
            var spotify = new SpotifyClient(token);
            
            var request = new PlayerPausePlaybackRequest();
            
            await spotify.Player.PausePlayback(request, cancellationToken);
            
        }
        catch (Exception e)
        {
            Console.WriteLine($"From SpotifyService.Pause: {e.Message}");
        }
    }
    
    
    public async Task Play(CancellationToken cancellationToken = default)
    {
        try
        {
            var token = await GetAuthRequestToken(cancellationToken);
            if (token is null) return;

            SpotifyClientConfig.CreateDefault().WithToken(token.AccessToken);
            
            var spotify = new SpotifyClient(token);
            
            var request = new PlayerResumePlaybackRequest();
            
            await spotify.Player.ResumePlayback(request, cancellationToken);
            
        }
        catch (Exception e)
        {
            Console.WriteLine($"From SpotifyService.Play: {e.Message}");
        }
    }
    
    
        
    public async Task SkipForward(CancellationToken cancellationToken = default)
    {
        try
        {
            var token = await GetAuthRequestToken(cancellationToken);
            if (token is null) return;

            SpotifyClientConfig.CreateDefault().WithToken(token.AccessToken);
            
            var spotify = new SpotifyClient(token);
            
            var request = new PlayerSkipNextRequest();
            
            await spotify.Player.SkipNext(request, cancellationToken);
            
        }
        catch (Exception e)
        {
            Console.WriteLine($"From SpotifyService.SkipForward: {e.Message}");
        }
    }
    
    
        
    public async Task SkipBackward(CancellationToken cancellationToken = default)
    {
        try
        {
            var token = await GetAuthRequestToken(cancellationToken);
            if (token is null) return;

            SpotifyClientConfig.CreateDefault().WithToken(token.AccessToken);
            
            var spotify = new SpotifyClient(token);
            
            var request = new PlayerSkipPreviousRequest();
            
            await spotify.Player.SkipPrevious(request, cancellationToken);
        }
        catch (Exception e)
        {
            Console.WriteLine($"From SpotifyService.SkipBackward: {e.Message}");
        }
    }



    public async Task SetVolume(int level, CancellationToken cancellationToken = default)
    {
        if (level < 0 || level > 100) return;
        
        try
        {
            var token = await GetAuthRequestToken(cancellationToken);
            if (token is null) return;

            SpotifyClientConfig.CreateDefault().WithToken(token.AccessToken);
            
            var spotify = new SpotifyClient(token);
            
            var request = new PlayerVolumeRequest(level);
            
            await spotify.Player.SetVolume(request, cancellationToken);
        }
        catch (Exception e)
        {
            Console.WriteLine($"From SpotifyService.SetVolume: {e.Message}");
        }
    }
    
    
    
    
    
    
    public async Task SetOAuthCode(string code)
    {
        _oauthCode = code;
        await _localStorage.SetItemAsync("OAuthCode", _protector.Protect(code));
        _ = await GetAuthRequestToken();
    }

    private async Task<string?> GetOAuthCode()
    {
        if (!string.IsNullOrEmpty(_oauthCode)) return _oauthCode;
        
        var protectedData = await _localStorage.GetItemAsync<string>("OAuthCode");
        return null;
        if (protectedData is null) return null;
        
        var data = _protector.Unprotect(protectedData);
        return data;
    }


    private async Task SetOAuthToken(AuthorizationCodeTokenResponse token)
    {
        _oauthToken = token;

        var serializedData = JsonConvert.SerializeObject(token);

        try
        {
            await _localStorage.SetItemAsStringAsync("spotify_o", _protector.Protect(serializedData));
        }
        catch {}
    }

    private async Task<AuthorizationCodeTokenResponse?> GetAuthTokenFromBrowser()
    {
        try
        {
            var serializedData = await _localStorage.GetItemAsStringAsync("spotify_o");
            if (string.IsNullOrEmpty(serializedData)) return null;
            
            var data = JsonConvert.DeserializeObject<AuthorizationCodeTokenResponse>(_protector.Unprotect(serializedData));
            return data;
        }
        catch(Exception exc)
        {
            return null;
        }
    }


    private async Task<ClientCredentialsTokenResponse> GetClientRequestToken(CancellationToken cancellationToken = default)
    {
        if (_clientToken is not null && !_clientToken.IsExpired) return _clientToken;
        
        var config = SpotifyClientConfig.CreateDefault();
        var request = new ClientCredentialsRequest(_clientId, _secret);

        _clientToken = await new OAuthClient(config).RequestToken(request, cancellationToken);

        return _clientToken;
    }


    private async Task<AuthorizationCodeTokenResponse?> GetAuthRequestToken(CancellationToken cancellationToken = default)
    {
        // _oauthToken ??= await GetAuthTokenFromBrowser();
        
        if (_oauthToken is not null && !_oauthToken.IsExpired) return _oauthToken;

        var oauthCode = await GetOAuthCode();
        
        if (string.IsNullOrEmpty(oauthCode)) return null;
        
        var config = SpotifyClientConfig.CreateDefault();

        try
        {
            var request = new AuthorizationCodeTokenRequest(_clientId, _secret, oauthCode, LoginCallbackUri);
            var token = await new OAuthClient(config).RequestToken(request, cancellationToken);
            
            await SetOAuthToken(token);
            
            return token;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return null;
        }
    }
    
}