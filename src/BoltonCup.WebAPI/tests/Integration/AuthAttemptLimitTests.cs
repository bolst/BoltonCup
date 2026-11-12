using System.Net;
using System.Text;
using System.Text.Json;
using BoltonCup.Application.Services;
using BoltonCup.Shared;
using BoltonCup.WebAPI.Auth;
using BoltonCup.WebAPI.RateLimiting;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace BoltonCup.WebAPI.Tests.Integration;

// Real pipeline, stubbed user service: the limit applies before any user lookup, so registered and unknown emails
// answer alike.
public class AuthAttemptLimitTests : IClassFixture<TestWebApplicationFactory>
{
    const string ProxyIp = "10.203.0.2";
    const string Email = "player@example.com";

    readonly TestWebApplicationFactory _factory;

    public AuthAttemptLimitTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    WebApplicationFactory<Program> CreateHost() =>
        _factory.WithWebHostBuilder(builder => builder
            .UseSetting($"{TrustedProxyPolicy.KnownNetworksPath}:0", $"{ProxyIp}/32")
            .ConfigureTestServices(services => services.AddScoped(_ => Mock.Of<IUserService>())));

    static Task<HttpContext> LoginAsync(WebApplicationFactory<Program> host, string client) =>
        host.Server.SendAsync(context =>
        {
            context.Connection.RemoteIpAddress = IPAddress.Parse(ProxyIp);
            context.Request.Headers["X-Forwarded-For"] = client;
            context.Request.Method = HttpMethods.Post;
            context.Request.Path = "/api/auth/login";
            context.Request.ContentType = "application/json";
            var body = JsonSerializer.Serialize(new { email = Email, password = "Wrong-Password-1" });
            context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));
        });

    static Task<string> ReadBodyAsync(HttpContext context) => new StreamReader(context.Response.Body).ReadToEndAsync();

    [Fact]
    public async Task Login_PastTheCallerBudget_Returns429LockedOut_WithoutBlockingOtherCallers()
    {
        using var host = CreateHost();
        var perCaller = AuthAttemptLimiter.LoginLimits.PerCaller.PermitLimit;
        for (var i = 0; i < perCaller; i++)
        {
            var allowed = await LoginAsync(host, "203.0.113.1");
            allowed.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
        }

        var refused = await LoginAsync(host, "203.0.113.1");
        var otherCaller = await LoginAsync(host, "203.0.113.2");

        refused.Response.StatusCode.Should().Be(StatusCodes.Status429TooManyRequests);
        (await ReadBodyAsync(refused)).Should().Contain(ErrorTypes.Auth.LockedOut);
        otherCaller.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
    }
}