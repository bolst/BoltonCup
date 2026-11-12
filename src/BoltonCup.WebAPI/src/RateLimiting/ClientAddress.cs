using System.Net;
using System.Net.Sockets;

namespace BoltonCup.WebAPI.RateLimiting;

/// <summary>Builds rate-limit keys from a client's IP address.</summary>
public static class ClientAddress
{
    const string Anonymous = "anon";

    /// <summary>
    /// Returns the address as a partition key. IPv6 clients are keyed by their /64, the smallest block an ISP hands
    /// out, so one client can't rotate through addresses to get fresh limits.
    /// </summary>
    public static string PartitionKey(IPAddress? address)
    {
        if (address is null)
        {
            return Anonymous;
        }

        if (address.IsIPv4MappedToIPv6)
        {
            address = address.MapToIPv4();
        }

        if (address.AddressFamily != AddressFamily.InterNetworkV6)
        {
            return address.ToString();
        }

        var bytes = address.GetAddressBytes();
        Array.Clear(bytes, 8, 8);
        return $"{new IPAddress(bytes)}/64";
    }
}