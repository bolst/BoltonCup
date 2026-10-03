using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace BoltonCup.WebAPI.Tests.Integration;

public class FranchiseEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    readonly WebApplicationFactory<Program> _factory;

    public FranchiseEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Theory]
    [InlineData("/api/franchises")]
    [InlineData("/api/franchises/bolton-bears")]
    public async Task FranchiseReads_AllowAnonymous(string path)
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync(path);

        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
        response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateTeamSongs_WithoutAuth_Returns401()
    {
        using var client = _factory.CreateClient();

        var response = await client.PutAsJsonAsync("/api/teams/1/songs", new { });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData("POST", "/api/franchises")]
    [InlineData("PUT", "/api/franchises/1")]
    [InlineData("DELETE", "/api/franchises/1")]
    public async Task FranchiseWrites_DoNotExist(string method, string path)
    {
        using var client = _factory.CreateClient();
        using var request = new HttpRequestMessage(new HttpMethod(method), path);

        var response = await client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed);
    }
}