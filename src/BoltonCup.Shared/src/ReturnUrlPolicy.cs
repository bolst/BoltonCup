using System;

namespace BoltonCup.Shared;

public static class ReturnUrlPolicy
{
    public const string DefaultRootDomain = "boltoncup.ca";

    /// <summary>
    /// Returns <paramref name="raw"/> resolved against <paramref name="baseUri"/> as an absolute URL when it is an
    /// https URL on <paramref name="rootDomain"/> or one of its subdomains; otherwise <c>null</c>.
    /// </summary>
    public static string? Sanitize(string? raw, Uri baseUri, string rootDomain, bool allowLocalhost)
    {
        // Browsers treat "\" as "/", so "/\evil.com" can mean "//evil.com" to them but not to Uri.
        if (string.IsNullOrWhiteSpace(raw) || raw.Contains('\\') || !Uri.TryCreate(baseUri, raw, out var uri))
        {
            return null;
        }

        if (!string.IsNullOrEmpty(uri.UserInfo))
        {
            return null;
        }

        var isLocalhost = allowLocalhost && uri.Host is ("localhost" or "127.0.0.1");
        var isAllowedScheme = uri.Scheme == Uri.UriSchemeHttps || (uri.Scheme == Uri.UriSchemeHttp && isLocalhost);
        if (!isAllowedScheme)
        {
            return null;
        }

        var isOnRootDomain = !string.IsNullOrEmpty(rootDomain)
            && (string.Equals(uri.Host, rootDomain, StringComparison.OrdinalIgnoreCase)
                || uri.Host.EndsWith("." + rootDomain, StringComparison.OrdinalIgnoreCase));

        return isOnRootDomain || isLocalhost
            ? uri.AbsoluteUri
            : null;
    }
}