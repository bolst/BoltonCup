using System.Net;
using System.Text;
using BoltonCup.WebAPI.Auth;
using BoltonCup.WebAPI.RateLimiting;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Xunit;

namespace BoltonCup.WebAPI.Tests.Integration;

// Drives the real pipeline with a chosen socket peer, so forwarded headers are trusted only from the configured proxy.
public class TrustedProxyTests : IClassFixture<TestWebApplicationFactory>
{
    const string ProxyIp = "10.203.0.2";
    const string AdminKey = "test-admin-key";
    const string CodeCheckPath = "/api/auth/verifyPasswordResetCode";

    readonly TestWebApplicationFactory _factory;

    public TrustedProxyTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    WebApplicationFactory<Program> CreateHost(string knownNetwork) =>
        _factory.WithWebHostBuilder(builder => builder
            .UseSetting($"{TrustedProxyPolicy.KnownNetworksPath}:0", knownNetwork)
            .UseSetting(ApiKeyConstants.AppSettingsPath, AdminKey));

    static Task<HttpContext> SendAsync(
        WebApplicationFactory<Program> host,
        string method,
        string path,
        string peer,
        string? forwardedFor = null,
        string? apiKey = null) =>
        host.Server.SendAsync(context =>
        {
            context.Connection.RemoteIpAddress = IPAddress.Parse(peer);
            context.Request.Method = method;
            context.Request.Path = path;
            if (forwardedFor is not null)
            {
                context.Request.Headers["X-Forwarded-For"] = forwardedFor;
            }

            if (apiKey is not null)
            {
                context.Request.Headers[ApiKeyConstants.Header] = apiKey;
            }

            if (method == HttpMethods.Post)
            {
                context.Request.ContentType = "application/json";
                context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes("{}"));
            }
        });

    [Fact]
    public async Task ThroughTrustedProxy_EachForwardedClientGetsItsOwnRateLimit()
    {
        using var host = CreateHost($"{ProxyIp}/32");

        for (var i = 0; i <= CodeCheckPolicy.PermitLimit; i++)
        {
            var context = await SendAsync(host, HttpMethods.Post, CodeCheckPath, ProxyIp, $"203.0.113.{i + 1}");

            context.Response.StatusCode.Should().NotBe(StatusCodes.Status429TooManyRequests);
        }
    }

    [Fact]
    public async Task FromUntrustedPeer_ForgedForwardedForDoesNotDodgeRateLimit()
    {
        using var host = CreateHost($"{ProxyIp}/32");

        HttpContext? last = null;
        for (var i = 0; i <= CodeCheckPolicy.PermitLimit; i++)
        {
            last = await SendAsync(host, HttpMethods.Post, CodeCheckPath, "198.51.100.7", $"203.0.113.{i + 1}");
        }

        last!.Response.StatusCode.Should().Be(StatusCodes.Status429TooManyRequests);
    }

    [Fact]
    public async Task AdminKey_ForwardedTailnetAddressThroughProxy_IsRejected()
    {
        using var host = CreateHost($"{ProxyIp}/32");

        var context = await SendAsync(host, HttpMethods.Get, "/api/admin/masquerade/search", ProxyIp, "100.64.0.1", AdminKey);

        context.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
    }

    [Fact]
    public async Task AdminKey_DirectTailnetPeer_IsAccepted()
    {
        using var host = CreateHost($"{ProxyIp}/32");

        var context = await SendAsync(host, HttpMethods.Get, "/api/admin/masquerade/search", "100.64.0.1", apiKey: AdminKey);

        context.Response.StatusCode.Should().NotBe(StatusCodes.Status401Unauthorized)
            .And.NotBe(StatusCodes.Status403Forbidden);
    }

    [Fact]
    public void InvalidKnownNetwork_FailsStartup()
    {
        using var host = CreateHost("not-a-cidr");

        var act = () => host.CreateClient();

        act.Should().Throw<Exception>().Which.ToString().Should().Contain("Invalid CIDR");
    }
}