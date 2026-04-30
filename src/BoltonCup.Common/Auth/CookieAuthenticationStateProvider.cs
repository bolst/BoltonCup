using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using BoltonCup.Sdk;
using BoltonCup.Shared;

namespace BoltonCup.Common.Auth;

public class CookieAuthenticationStateProvider(IBoltonCupApi _api) : AuthenticationStateProvider
{
    private static AuthenticationState AnonymousUser => new(new ClaimsPrincipal(new ClaimsIdentity()));
    
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var currentUser = await _api.GetCurrentUserAsync();
            if (currentUser is not { IsAuthenticated: true })
                return AnonymousUser;
            
            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, currentUser.Name),
                new(ClaimTypes.Email, currentUser.Email),
                new(ClaimTypes.Role, string.Join(';', currentUser.Roles)),
            };

            var accountIdString = currentUser.AccountId?.ToString();
            if (!string.IsNullOrEmpty(accountIdString))
                claims.Add(new Claim(BoltonCupClaimTypes.AccountId, accountIdString));
            
            claims.AddRange(
                currentUser.TeamGmIds
                    .Select(id => new Claim(BoltonCupClaimTypes.TeamGm, id.ToString()))
            );
            claims.AddRange(
                currentUser.TournamentGmIds
                    .Select(id => new Claim(BoltonCupClaimTypes.TournamentGm, id.ToString()))
            );
            
            var identity = new ClaimsIdentity(claims, "ServerCookie");
            return new AuthenticationState(new ClaimsPrincipal(identity));
        }
        catch
        {
            // 401 Unauthorized or Network error -> Not Logged In
            // TODO: log
        }

        return AnonymousUser;
    }
}