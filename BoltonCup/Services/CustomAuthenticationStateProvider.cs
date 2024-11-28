using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Supabase.Gotrue;
using Supabase.Gotrue.Interfaces;

namespace BoltonCup.Data;

/// <summary>
/// Creates a link between <see cref="Supabase"/> and <see cref="AuthenticatedState"/> to provide support for using
/// Gotrue with Blazor's built in Authentication handler.
/// </summary>
public class CustomAuthenticationStateProvider : AuthenticationStateProvider, IDisposable
{
    private readonly Supabase.Client SBClient;
    private readonly CustomUserService UserService;

    private AuthenticationState AnonymousState => new(new ClaimsPrincipal(new ClaimsIdentity()));

    public CustomAuthenticationStateProvider(Supabase.Client _SBClient, CustomUserService _userService)
    {
        Console.WriteLine($"{nameof(CustomAuthenticationStateProvider)} initialized.");
        SBClient = _SBClient;
        UserService = _userService;
        SBClient.Auth.AddStateChangedListener(SupabaseAuthStateChanged);
    }

    public void Dispose()
    {
        SBClient.Auth.RemoveStateChangedListener(SupabaseAuthStateChanged);
    }

    /// <summary>
    /// Adds a listener on the supabase client that will notify components of a change in authentication state in realtime.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="state"></param>
    private async void SupabaseAuthStateChanged(IGotrueClient<User, Session> sender, Constants.AuthState state)
    {
        switch (state)
        {
            case Constants.AuthState.SignedIn:
                {
                    BCUser? user = await UserService.LookupUserInDatabase(SBClient.Auth.CurrentUser!.Email);
                    if (user is null)
                    {
                        NotifyAuthenticationStateChanged(Task.FromResult(AnonymousState));
                    }
                    else
                    {
                        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user.ToClaimsPrincipal())));
                    }
                    break;
                }
            case Constants.AuthState.SignedOut:
                NotifyAuthenticationStateChanged(Task.FromResult(AnonymousState));
                break;
        }
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        await Task.Run(() => SBClient.Auth.LoadSession());

        if (SBClient.Auth.CurrentUser == null)
        {
            Console.WriteLine("An authenticated user not found, returning as anonymous.");
            return AnonymousState;
        }

        var currentUser = SBClient.Auth.CurrentUser;
        if (currentUser is null) return AnonymousState;

        BCUser? user = await UserService.LookupUserInDatabase(currentUser!.Email);
        if (user == null) return AnonymousState;

        var principal = user.ToClaimsPrincipal();

        return new AuthenticationState(principal);
    }

    public async Task<bool> IsAuthenticated()
    {
        var state = await GetAuthenticationStateAsync();
        return state.User.Identity is not null && state.User.Identity.IsAuthenticated;
    }

    public async Task SignOutAsync()
    {
        await SBClient.Auth.SignOut();
    }
}