using System.Net;
using System.Security.Claims;
using System.Text.Encodings.Web;
using BoltonCup.Shared;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace BoltonCup.WebAPI.Auth;

/// <summary>Handles API key authentication by validating the <c>BoltonCup-Api-Key</c> request header.</summary>
public class ApiKeyAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    /// <inheritdoc/>
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // ensure header is present
        if (!Request.Headers.TryGetValue(ApiKeyConstants.Header, out var extractedApiKey))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        // validate
        var config = Context.RequestServices.GetRequiredService<IConfiguration>();
        var adminKey = config[ApiKeyConstants.AppSettingsPath];
        if (string.IsNullOrEmpty(adminKey) || extractedApiKey != adminKey)
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid API Key"));
        }

        // Restrict the key to trusted networks (e.g. the Tailscale tailnet). Use the true socket peer
        // captured before UseForwardedHeaders so a public caller can't spoof it via X-Forwarded-For.
        var allowedNetworks = config.GetSection(ApiKeyConstants.AllowedNetworksPath).Get<string[]>();
        if (allowedNetworks is null || allowedNetworks.Length == 0)
        {
            allowedNetworks = AdminApiKeyNetworkPolicy.DefaultAllowedNetworks;
        }
        var peerIp = Context.Items[ApiKeyConstants.TrueRemoteIpItemKey] as IPAddress
            ?? Context.Connection.RemoteIpAddress;
        if (!AdminApiKeyNetworkPolicy.IsAllowed(peerIp, allowedNetworks))
        {
            // Logs the observed source so a blocked-but-legit caller (e.g. Docker masking the IP) is diagnosable.
            Logger.LogWarning("Admin API key rejected: source {PeerIp} not in allowed networks.", peerIp);
            return Task.FromResult(AuthenticateResult.Fail("API key not permitted from this network."));
        }

        // create identity
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "bc-admin"),
            new(ClaimTypes.Name, "BC Admin"),
            new(ClaimTypes.Role, "Admin")
        };

        // Acts as a real account so completed-account-gated endpoints (e.g. asset uploads) accept the key.
        var adminAccountId = config[ApiKeyConstants.AdminAccountIdPath];
        if (!string.IsNullOrEmpty(adminAccountId))
        {
            claims.Add(new Claim(BoltonCupClaimTypes.AccountId, adminAccountId));
        }
        var identity = new ClaimsIdentity(claims, ApiKeyConstants.Scheme);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, ApiKeyConstants.Scheme);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}