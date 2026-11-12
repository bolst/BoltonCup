using System.Net;
using FluentAssertions;
using Xunit;

namespace BoltonCup.WebAPI.Tests.Integration;

// GlobalRateLimiter allows 100 requests/minute per user-or-IP (sliding window); requests beyond
// that are rejected by RateLimitResponder with a 429 + BoltonCupProblemDetails body.
public class RateLimiterTests : IClassFixture<TestWebApplicationFactory>
{
    readonly TestWebApplicationFactory _factory;

    public RateLimiterTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task ExceedingRateLimit_Returns429WithProblemDetails()
    {
        using var client = _factory.CreateClient();

        HttpResponseMessage? lastResponse = null;
        for (var i = 0; i < 101; i++)
        {
            lastResponse = await client.GetAsync("/health");
        }

        lastResponse!.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);

        var body = await lastResponse.Content.ReadAsStringAsync();
        body.Should().Contain("Rate limit exceeded");
    }
}
