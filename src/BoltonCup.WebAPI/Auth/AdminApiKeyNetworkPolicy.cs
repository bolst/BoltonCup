using System.Net;

namespace BoltonCup.WebAPI.Auth;

/// <summary>Decides whether the admin API key may be used from a given source address.</summary>
public static class AdminApiKeyNetworkPolicy
{
    /// <summary>
    /// Used when no allow-list is configured: the Tailscale tailnet only. Loopback/LAN are deliberately
    /// excluded so a production reverse proxy / tunnel forwarding over loopback can't slip past the check.
    /// Add loopback explicitly (e.g. in appsettings.Development.json) for local testing.
    /// </summary>
    public static readonly string[] DefaultAllowedNetworks =
    [
        "100.64.0.0/10", // Tailscale IPv4 (CGNAT)
        "fd7a:115c:a1e0::/48", // Tailscale IPv6 (ULA)
    ];

    /// <summary>True if <paramref name="address"/> falls within any of the allowed CIDR ranges.</summary>
    public static bool IsAllowed(IPAddress? address, IReadOnlyList<string> allowedNetworks)
    {
        if (address is null)
        {
            return false;
        }

        // A v4 connection can surface as an IPv4-mapped IPv6 address; compare on the v4 form.
        var ip = address.IsIPv4MappedToIPv6 ? address.MapToIPv4() : address;

        foreach (var cidr in allowedNetworks)
        {
            if (IPNetwork.TryParse(cidr, out var network) && network.Contains(ip))
            {
                return true;
            }
        }

        return false;
    }
}