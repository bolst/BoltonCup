using BoltonCup.Core;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Xunit;

namespace BoltonCup.WebAPI.Tests.Integration;

public class TelemetryTests : IClassFixture<TestWebApplicationFactory>
{
    readonly TestWebApplicationFactory _factory;

    public TelemetryTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task App_BootsWithTelemetryAndNoOtlpEndpoint()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/health");

        response.IsSuccessStatusCode.Should().BeTrue();
        _factory.Services.GetService<ITelemetry>().Should().NotBeNull();
        _factory.Services.GetService<TracerProvider>().Should().NotBeNull();
        _factory.Services.GetService<MeterProvider>().Should().NotBeNull();
    }
}
