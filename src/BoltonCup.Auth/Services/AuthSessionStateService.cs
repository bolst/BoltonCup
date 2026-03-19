using BoltonCup.Auth.Extensions;
using BoltonCup.Auth.Models;
using BoltonCup.Common;
using BoltonCup.Sdk;
using BoltonCup.SessionStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;

namespace BoltonCup.Auth.Services;

public class AuthSessionStateService(
    NavigationManager _navigation, 
    IBoltonCupApi _api,
    IOptions<BoltonCupConfiguration> _config,
    ISessionStorageService _sessionStorage
)
{
    private const string _sessionKey = "authapi";
    private AuthSessionContext _internalContext { get; set; } = new();
    public IAuthSessionContext Context => _internalContext;


    public async Task TryLoadFromSessionAsync()
    {
        _internalContext = await _sessionStorage.GetItemAsync<AuthSessionContext>(_sessionKey);
    }
    
    public async Task LogInOrSignUp(LogInOrSignUpFormModel? model = null)
    {
        try
        {
            if (model is not null)
            {
                _internalContext.Email = model.Email;
                await PersistContextAsync();
            }
            // will throw 204 or 404 if user does not exist
            _ = await _api.GetUserAsync(_internalContext.Email);
            _navigation.NavigateWithReturnUrl("log-in/password");
        }
        catch (ApiException e)
            when (e.StatusCode is 204 or 404)
        {
            _navigation.NavigateWithReturnUrl("create-account/password");
        }
    }
    
    public void NavigateToReturnUrlOrDefault()
    {
        var returnUrl = _navigation.GetReturnUrl() ?? _config.Value.WebBaseUrl;
        _navigation.NavigateTo(returnUrl, forceLoad: true);
    }

    public void Reset()
    {
       _navigation.NavigateWithReturnUrl("log-in-or-sign-up");
    }

    private async Task PersistContextAsync()
    {
        await _sessionStorage.SetItemAsync(_sessionKey, _internalContext);
    }

    private class AuthSessionContext : IAuthSessionContext
    {
        public string? Email { get; set; }
    }
}