using BoltonCup.Auth.Models;
using BoltonCup.Common;
using BoltonCup.Sdk;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;

namespace BoltonCup.Auth.Services;

public class AuthSessionStateService(
    NavigationManager _navigation, 
    IBoltonCupApi _api,
    IOptions<BoltonCupConfiguration> _config
)
{
    public string? Email { get; private set; }
    
    public async Task Initialize(LogInOrSignUpForm model)
    {
        try
        {
            Email = model.Email;
            // will throw 204 or 404 if user does not exist
            _ = await _api.GetUserAsync(model.Email);
            NavigateWithReturnUrl("log-in/password");
        }
        catch (ApiException e)
            when (e.StatusCode is 204 or 404)
        {
            NavigateWithReturnUrl("create-account/password");
        }
    }

    public async Task CreateAccount(CreateAccountWithPasswordForm model)
    {
        var delayTask = Task.Delay(3000); // load for 3 seconds minimum
        var signUpTask = _api.RegisterAsync(new RegisterRequest
        {
            Email = model.Email,
            Password = model.Password
        });
        await Task.WhenAll(delayTask, signUpTask);
        NavigateToReturnUrlOrDefault();
    }

    public async Task LogIn(LogInWithPasswordForm model)
    {
        var delayTask = Task.Delay(3000); // load for 3 seconds minimum
        var loginTask = _api.LoginWithCookieAsync(new LoginWithCookieRequest
        {
            Email = model.Email,
            Password = model.Password
        });
        await Task.WhenAll(delayTask, loginTask);
        NavigateToReturnUrlOrDefault();
    }

    public void Reset()
    {
        NavigateWithReturnUrl("log-in-or-sign-up");
    }

    private string? GetReturnUrl()
    {
        var uriBuilder = new UriBuilder(_navigation.Uri);
        var query = QueryHelpers.ParseQuery(uriBuilder.Query);
        return query.GetValueOrDefault("returnUrl");
    }

    private void NavigateWithReturnUrl(string destination)
    {
        var returnUrl = GetReturnUrl();
        if (!string.IsNullOrEmpty(returnUrl))
            destination += "?returnUrl=" + returnUrl;
        _navigation.NavigateTo(destination);
    }

    private void NavigateToReturnUrlOrDefault()
    {
        var returnUrl = GetReturnUrl() ?? _config.Value.WebBaseUrl;
        _navigation.NavigateTo(returnUrl, forceLoad: true);
    }
}