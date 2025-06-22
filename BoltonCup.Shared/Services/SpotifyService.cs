using SpotifyAPI.Web;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.DataProtection;

namespace BoltonCup.Shared.Data;

public class SpotifyService
{

    private readonly string _clientId;
    private readonly string _secret;
    private readonly ILocalStorageService _localStorage;
    private readonly IDataProtector _protector;
    private readonly IBCData _bcData;
    private readonly SpotifyClientConfig _defaultConfig;
    
    private ClientCredentialsTokenResponse? _clientToken;
    private IRefreshableToken? _oauthToken;
    private string? _oauthCode;
    
    private readonly Uri LoginCallbackUri = new("https://127.0.0.1:7107/callback/");
    
    public Uri LoginRequestUri => new LoginRequest(LoginCallbackUri, _clientId, LoginRequest.ResponseType.Code)
    {
        Scope = [
            Scopes.UserReadPlaybackState, 
            Scopes.UserModifyPlaybackState, 
            Scopes.UserReadCurrentlyPlaying,
            Scopes.PlaylistModifyPublic,
        ],
    }.ToUri();


    
    public SpotifyService(string clientId, string secret, ILocalStorageService localStorage, IDataProtectionProvider provider, IBCData bcData)
    {
        _clientId = clientId;
        _secret = secret;
        _localStorage = localStorage;
        _protector = provider.CreateProtector("creds");
        _bcData = bcData;
        _defaultConfig = SpotifyClientConfig.CreateDefault();
    }


    public async Task LogoutAsync()
    {
        await _localStorage.RemoveItemAsync("local_id");
        _oauthToken = null;
    }
    

    public async Task<SpotifyClient?> GetClientAsync()
    {
        var token = await GetAuthRequestToken();

        var config = _defaultConfig;
        
        if (token is not null)
        {
            config = config.WithToken(token);
        }

        try
        {
            return new SpotifyClient(config);
        }
        catch (Exception e)
        {
            Console.WriteLine($"From GetClientAsync:\n{e.Message}");
            return null;
        }
    }
    
    
    public async Task<SearchResponse> SearchSongs(string song, int limit = 10, CancellationToken cancellationToken = default)
    {
        try
        {
            var token = await GetClientRequestToken(cancellationToken);
            
            var spotify = new SpotifyClient(_defaultConfig.WithToken(token.AccessToken));
            
            var request = new SearchRequest(SearchRequest.Types.Track, $"{song}");

            return await spotify.Search.Item(request, cancel: cancellationToken);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error in Spotify Search: {e.Message}");
            return new();
        }
    }
    
    
    
    
    
    public async Task ConfigAuthWithCallbackCode(string code)
    {
        try
        {
            var request = new AuthorizationCodeTokenRequest(_clientId, _secret, code, LoginCallbackUri);
            _oauthToken = await new OAuthClient(_defaultConfig).RequestToken(request);

            if (_oauthToken.IsExpired) return;
            
            var localIdStr = await _localStorage.GetItemAsync<string>("local_id");
            // if local storage has no value or is improper guid: we generate a new one for local storage
            // use this to set refresh token in db
            if (localIdStr is null || !Guid.TryParse(localIdStr, out Guid localId))
            {
                localId = Guid.NewGuid();
                await _localStorage.SetItemAsync("local_id", localId.ToString());
            }

            await _bcData.UpdateRefreshToken(localId, _oauthToken.RefreshToken);
        }
        catch (Exception e)
        {
            Console.WriteLine($"From ConfigAuthWithCallbackCode:\n{e.Message}");
        }
    }

    private async Task<ClientCredentialsTokenResponse> GetClientRequestToken(CancellationToken cancellationToken = default)
    {
        if (_clientToken is not null && !_clientToken.IsExpired) return _clientToken;
        
        var request = new ClientCredentialsRequest(_clientId, _secret);
        _clientToken = await new OAuthClient(_defaultConfig).RequestToken(request, cancellationToken);

        return _clientToken;
    }


    private async Task<string?> GetAuthRequestToken(CancellationToken cancellationToken = default)
    {
        var localIdStr = await _localStorage.GetItemAsync<string>("local_id", cancellationToken: cancellationToken);
        // if local storage has no value or is improper guid: we generate a new one for local storage
        // use this to lookup refresh token in db
        if (localIdStr is null || !Guid.TryParse(localIdStr, out Guid localId))
        {
            localId = Guid.NewGuid();
            await _localStorage.SetItemAsync("local_id", localId.ToString(), cancellationToken);
        }

        // if oauth token is valid just return that
        if (_oauthToken is not null && !_oauthToken.IsExpired)
        {
            if (!string.IsNullOrEmpty(_oauthToken.RefreshToken))
                await _bcData.UpdateRefreshToken(localId, _oauthToken.RefreshToken);
            return _oauthToken.AccessToken;
        }

        // use local id to try to fetch refresh token
        var refreshToken = await _bcData.GetRefreshToken(localId);

        if (refreshToken is not null)
        {
            try
            {
                var authRequest = new AuthorizationCodeRefreshRequest(_clientId, _secret, refreshToken.token);
                _oauthToken = await new OAuthClient(_defaultConfig).RequestToken(authRequest, cancellationToken);
                return _oauthToken.AccessToken;
            }
            catch (Exception e)
            {
                Console.WriteLine($"GetAuthRequestToken:\n{e.Message}");
            }
        }

        return null;
    }
}