using Microsoft.AspNetCore.HttpOverrides;

namespace BoltonCup.WebAPI.Auth;

/// <summary>Decides which peers may set the client IP and scheme through forwarded headers.</summary>
public static class TrustedProxyPolicy
{
    /// <summary>Configuration path of the CIDR list of proxies whose forwarded headers are trusted.</summary>
    public const string KnownNetworksPath = "BoltonCup:ForwardedHeaders:KnownNetworks";

    /// <summary>
    /// Trusts <c>X-Forwarded-For</c>/<c>X-Forwarded-Proto</c> only from <paramref name="knownNetworks"/>, or only from
    /// loopback (the framework default) when none are given. Only the right-most <c>X-Forwarded-For</c> entry, the one
    /// the proxy appended, is used.
    /// </summary>
    /// <exception cref="FormatException">A non-blank entry is not a valid CIDR.</exception>
    public static void Apply(ForwardedHeadersOptions options, IEnumerable<string>? knownNetworks)
    {
        options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        options.ForwardLimit = 1;
        var configuredNetworks = (knownNetworks ?? []).Where(cidr => !string.IsNullOrWhiteSpace(cidr)).ToList();
        if (configuredNetworks.Count == 0)
        {
            return;
        }

        options.KnownIPNetworks.Clear();
        options.KnownProxies.Clear();
        foreach (var cidr in configuredNetworks)
        {
            if (!System.Net.IPNetwork.TryParse(cidr, out var network))
            {
                throw new FormatException($"Invalid CIDR '{cidr}' in {KnownNetworksPath}.");
            }

            options.KnownIPNetworks.Add(network);
        }
    }
}