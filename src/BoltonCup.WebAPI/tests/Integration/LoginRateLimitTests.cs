using System.Net;
using System.Text;
using BoltonCup.WebAPI.RateLimiting;
using FluentAssertions;
using Xunit;

namespace BoltonCup.WebAPI.Tests.Integration;

// LoginPolicy allows PermitLimit login requests per minute per IP. An empty body fails model validation, so requests never
// reach the database.
public class LoginRateLimitTests : IClassFixture<TestWebApplicationFactory>
{
    readonly TestWebApplicationFactory _factory;

    public LoginRateLimitTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task ExceedingLoginLimit_Returns429WithProblemDetails()
    {
        using var client = _factory.CreateClient();

        HttpResponseMessage? lastResponse = null;
        for (var i = 0; i <= LoginPolicy.PermitLimit; i++)
        {
            lastResponse?.Dispose();
            using var content = new StringContent("{}", Encoding.UTF8, "application/json");
            lastResponse = await client.PostAsync("/api/auth/login", content);
            if (i < LoginPolicy.PermitLimit)
            {
                lastResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

        lastResponse!.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
        var body = await lastResponse.Content.ReadAsStringAsync();
        body.Should().Contain("Rate limit exceeded");
        lastResponse.Dispose();
    }
}