using System.Net;
using BoltonCup.WebAPI.Auth;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace BoltonCup.WebAPI.Tests.Auth;

public class TrustedProxyPolicyTests
{
    const string ProxyNetwork = "10.203.0.2/32";

    static async Task<HttpContext> RunAsync(string peer, string forwardedFor, params string[] knownNetworks)
    {
        var options = new ForwardedHeadersOptions();
        TrustedProxyPolicy.Apply(options, knownNetworks);
        var middleware = new ForwardedHeadersMiddleware(_ => Task.CompletedTask, NullLoggerFactory.Instance, Options.Create(options));

        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = IPAddress.Parse(peer);
        context.Request.Scheme = "http";
        context.Request.Headers["X-Forwarded-For"] = forwardedFor;
        context.Request.Headers["X-Forwarded-Proto"] = "https";
        await middleware.Invoke(context);

        return context;
    }

    [Fact]
    public async Task TrustedProxy_UsesRightMostForwardedAddressAndScheme()
    {
        var context = await RunAsync("10.203.0.2", "198.51.100.1, 203.0.113.9", ProxyNetwork);

        context.Connection.RemoteIpAddress.Should().Be(IPAddress.Parse("203.0.113.9"));
        context.Request.Scheme.Should().Be("https");
    }

    [Theory]
    [InlineData("203.0.113.50")] // public caller
    [InlineData("100.64.0.1")]   // tailnet caller
    [InlineData("10.203.0.1")]   // Docker gateway on the proxy network
    [InlineData("127.0.0.1")]    // loopback is not trusted once a proxy is configured
    public async Task UntrustedPeer_IsNotRewritten(string peer)
    {
        var context = await RunAsync(peer, "198.51.100.1", ProxyNetwork);

        context.Connection.RemoteIpAddress.Should().Be(IPAddress.Parse(peer));
        context.Request.Scheme.Should().Be("http");
    }

    [Fact]
    public async Task NoKnownNetworks_TrustsLoopbackOnly()
    {
        var publicPeer = await RunAsync("203.0.113.50", "198.51.100.1");
        var loopbackPeer = await RunAsync("127.0.0.1", "198.51.100.1");

        publicPeer.Connection.RemoteIpAddress.Should().Be(IPAddress.Parse("203.0.113.50"));
        loopbackPeer.Connection.RemoteIpAddress.Should().Be(IPAddress.Parse("198.51.100.1"));
    }

    [Fact]
    public async Task BlankKnownNetworks_FallBackToLoopbackOnly()
    {
        var context = await RunAsync("127.0.0.1", "198.51.100.1", " ");

        context.Connection.RemoteIpAddress.Should().Be(IPAddress.Parse("198.51.100.1"));
    }

    [Fact]
    public void InvalidCidr_Throws()
    {
        var act = () => TrustedProxyPolicy.Apply(new ForwardedHeadersOptions(), ["not-a-cidr"]);

        act.Should().Throw<FormatException>().WithMessage("*not-a-cidr*");
    }
}