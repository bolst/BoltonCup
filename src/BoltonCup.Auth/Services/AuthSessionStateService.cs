using BoltonCup.Auth.Models;
using BoltonCup.Common;
using BoltonCup.Sdk;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;

namespace BoltonCup.Auth.Services;

public class AuthSessionStateService(
    NavigationManager _navigation, 
    IBoltonCupApi _api,
    IOptions<BoltonCupConfiguration> _config
)
{
    public string? Email { get; private set; }
    
    public async Task Initialize(LogInOrSignUpForm model, string? returnUrl)
    {
        try
        {
            Email = model.Email;
            // will throw 204 or 404 if user does not exist
            _ = await _api.GetUserAsync(model.Email);
            NavigateTo($"log-in/password", returnUrl);
        }
        catch (ApiException e)
            when (e.StatusCode is 204 or 404)
        {
            NavigateTo($"create-account/password", returnUrl);
        }
    }

    public async Task CreateAccount(CreateAccountWithPasswordForm model, string? returnUrl)
    {
        var delayTask = Task.Delay(3000); // load for 3 seconds minimum
        var signUpTask = _api.RegisterAsync(new RegisterRequest
        {
            Email = model.Email,
            Password = model.Password
        });
        await Task.WhenAll(delayTask, signUpTask);
        _navigation.NavigateTo(returnUrl ?? _config.Value.WebBaseUrl, forceLoad: true);
    }

    public async Task LogIn(LogInWithPasswordForm model, string? returnUrl)
    {
        var delayTask = Task.Delay(3000); // load for 3 seconds minimum
        var loginTask = _api.LoginWithCookieAsync(new LoginWithCookieRequest
        {
            Email = model.Email,
            Password = model.Password
        });
        await Task.WhenAll(delayTask, loginTask);
        _navigation.NavigateTo(returnUrl ?? _config.Value.WebBaseUrl, true);
    }

    private void NavigateTo(string destination, string? returnUrl = null)
    {
        var nav = destination;
        if (!string.IsNullOrEmpty(returnUrl))
            nav += "?returnUrl=" + returnUrl;
        _navigation.NavigateTo(nav);
    }
}