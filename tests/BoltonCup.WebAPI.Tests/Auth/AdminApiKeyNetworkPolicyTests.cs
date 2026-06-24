using System.Net;
using BoltonCup.WebAPI.Auth;
using FluentAssertions;
using Xunit;

namespace BoltonCup.WebAPI.Tests.Auth;

public class AdminApiKeyNetworkPolicyTests
{
    private static readonly string[] Defaults = AdminApiKeyNetworkPolicy.DefaultAllowedNetworks;

    [Theory]
    [InlineData("100.126.217.83")] // Tailscale CGNAT
    [InlineData("100.64.0.1")]     // Tailscale CGNAT edge
    public void IsAllowed_AddressInDefaultRanges_ReturnsTrue(string address)
    {
        AdminApiKeyNetworkPolicy.IsAllowed(IPAddress.Parse(address), Defaults).Should().BeTrue();
    }

    [Theory]
    [InlineData("8.8.8.8")]        // public
    [InlineData("203.0.113.10")]   // public (TEST-NET-3)
    [InlineData("100.128.0.1")]    // just outside 100.64.0.0/10
    [InlineData("192.168.1.5")]    // private LAN, not in the tailnet-only default
    [InlineData("127.0.0.1")]      // loopback excluded from the default (prod foot-gun)
    [InlineData("172.17.0.1")]     // Docker bridge gateway
    public void IsAllowed_AddressOutsideRanges_ReturnsFalse(string address)
    {
        AdminApiKeyNetworkPolicy.IsAllowed(IPAddress.Parse(address), Defaults).Should().BeFalse();
    }

    [Fact]
    public void IsAllowed_LoopbackWithExplicitLoopbackRange_ReturnsTrue()
    {
        // Local dev opts loopback back in via config (e.g. appsettings.Development.json).
        string[] withLoopback = ["127.0.0.0/8", "::1/128"];
        AdminApiKeyNetworkPolicy.IsAllowed(IPAddress.Loopback, withLoopback).Should().BeTrue();
    }

    [Fact]
    public void IsAllowed_Ipv4MappedTailscaleAddress_ReturnsTrue()
    {
        var mapped = IPAddress.Parse("100.126.217.83").MapToIPv6();
        mapped.IsIPv4MappedToIPv6.Should().BeTrue();
        AdminApiKeyNetworkPolicy.IsAllowed(mapped, Defaults).Should().BeTrue();
    }

    [Fact]
    public void IsAllowed_TailscaleIpv6_ReturnsTrue()
    {
        AdminApiKeyNetworkPolicy.IsAllowed(IPAddress.Parse("fd7a:115c:a1e0::1"), Defaults).Should().BeTrue();
    }

    [Fact]
    public void IsAllowed_NullAddress_ReturnsFalse()
    {
        AdminApiKeyNetworkPolicy.IsAllowed(null, Defaults).Should().BeFalse();
    }

    [Fact]
    public void IsAllowed_EmptyAllowList_ReturnsFalse()
    {
        AdminApiKeyNetworkPolicy.IsAllowed(IPAddress.Loopback, []).Should().BeFalse();
    }
}
