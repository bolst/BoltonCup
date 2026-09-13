using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace BoltonCup.WebAPI.Tests.Integration;

// ForwardedHeadersOptions.KnownIPNetworks/KnownProxies are cleared (Program.cs), so the
// middleware processes X-Forwarded-For/X-Forwarded-Proto unconditionally. The true socket peer
// is still captured into HttpContext.Items before this middleware runs, specifically so the
// admin API-key network check can't be spoofed by these headers - that capture isn't observable
// over HTTP without a diagnostic endpoint, so this only asserts the pipeline stays healthy.
public class ForwardedHeadersTests : IClassFixture<WebApplicationFactory<Program>>
{
    readonly WebApplicationFactory<Program> _factory;

    public ForwardedHeadersTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task RequestWithForwardedHeaders_StillSucceeds()
    {
        using var client = _factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/health");
        request.Headers.Add("X-Forwarded-For", "203.0.113.7");
        request.Headers.Add("X-Forwarded-Proto", "https");

        var response = await client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
