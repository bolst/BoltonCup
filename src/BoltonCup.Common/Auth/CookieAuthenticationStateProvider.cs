using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using BoltonCup.Sdk;
using BoltonCup.Shared;

namespace BoltonCup.Common.Auth;

public class CookieAuthenticationStateProvider(IBoltonCupApi _api, ILogger<CookieAuthenticationStateProvider> _logger) : AuthenticationStateProvider
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
        catch (ApiException ex) when (ex is { StatusCode: 401 })
        {
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // 401 Unauthorized is expected when not logged in; anything else is unexpected
            _logger.LogWarning(ex, "Unexpected error fetching authentication state; treating as anonymous.");
        }

        return AnonymousUser;
    }
}