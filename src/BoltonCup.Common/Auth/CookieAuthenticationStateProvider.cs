using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using BoltonCup.Sdk;

namespace BoltonCup.Common.Auth;

public class CookieAuthenticationStateProvider(IBoltonCupApi _api) : AuthenticationStateProvider
{
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var userInfo = await _api.GetMeAsync();

            if (userInfo is { IsAuthenticated: true })
            {
                var claims = new List<Claim>
                {
                    new(ClaimTypes.Name, userInfo.Name),
                    new(ClaimTypes.Email, userInfo.Email),
                    new(ClaimTypes.Role, string.Join(';', userInfo.Roles))
                };
                var identity = new ClaimsIdentity(claims, "ServerCookie");
                return new AuthenticationState(new ClaimsPrincipal(identity));
            }
        }
        catch 
        {
            // 401 Unauthorized or Network error -> Not Logged In
        }

        return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
    }
}