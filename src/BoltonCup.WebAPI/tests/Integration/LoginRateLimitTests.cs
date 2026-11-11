using System.Net;
using System.Text;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace BoltonCup.WebAPI.Tests.Integration;

// LoginPolicy allows 20 login requests/minute per IP. An empty body fails model validation, so requests never
// reach the database.
public class LoginRateLimitTests : IClassFixture<WebApplicationFactory<Program>>
{
    readonly WebApplicationFactory<Program> _factory;

    public LoginRateLimitTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task ExceedingLoginLimit_Returns429WithProblemDetails()
    {
        using var client = _factory.CreateClient();

        HttpResponseMessage? lastResponse = null;
        for (var i = 0; i < 21; i++)
        {
            lastResponse?.Dispose();
            using var content = new StringContent("{}", Encoding.UTF8, "application/json");
            lastResponse = await client.PostAsync("/api/auth/login", content);
            if (i < 20)
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