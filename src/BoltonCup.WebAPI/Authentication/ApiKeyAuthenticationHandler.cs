using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace BoltonCup.WebAPI.Authentication;



public class ApiKeyAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string HeaderName = "BoltonCup-Api-Key";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // ensure header is present
        if (!Request.Headers.TryGetValue(HeaderName, out var extractedApiKey))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        // validate
        var config = Context.RequestServices.GetRequiredService<IConfiguration>();
        var adminKey = config["BoltonCup:Authentication:AdminApiKey"];
        if (string.IsNullOrEmpty(adminKey) || extractedApiKey != adminKey)
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid API Key"));
        }

        // create identity
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "bc-admin"),
            new Claim(ClaimTypes.Name, "BC Admin"),
            new Claim(ClaimTypes.Role, "Admin") 
        };
        var identity = new ClaimsIdentity(claims, ApiKeyConstants.Scheme);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, ApiKeyConstants.Scheme);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}