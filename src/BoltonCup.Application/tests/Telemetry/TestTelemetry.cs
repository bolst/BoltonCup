using BoltonCup.Application.Telemetry;
using BoltonCup.Core;
using Microsoft.Extensions.Logging.Abstractions;

namespace BoltonCup.Application.Tests.Telemetry;

static class TestTelemetry
{
    // The real implementation is inert without an ActivityListener/MeterListener attached.
    public static readonly ITelemetry Instance = new ActivityTelemetry(NullLogger<ActivityTelemetry>.Instance);
}
