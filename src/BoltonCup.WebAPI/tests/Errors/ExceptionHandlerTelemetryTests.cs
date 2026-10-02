using System.Text.Json;
using BoltonCup.Core;
using BoltonCup.Core.Exceptions;
using BoltonCup.Shared;
using BoltonCup.WebAPI.Errors;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace BoltonCup.WebAPI.Tests.Errors;

public class ExceptionHandlerTelemetryTests
{
    readonly Mock<ITelemetry> _telemetry = new();

    static DefaultHttpContext CreateContext() => new()
    {
        Response = { Body = new MemoryStream() },
        TraceIdentifier = "trace-123",
    };

    static async Task<JsonElement> ReadBodyAsync(HttpContext context)
    {
        context.Response.Body.Position = 0;
        return await JsonSerializer.DeserializeAsync<JsonElement>(context.Response.Body);
    }

    static bool HasAttribute(object?[] keyValues, string key, object? value) => keyValues
        .Chunk(2)
        .Any(pair => pair.Length == 2 && Equals(pair[0], key) && Equals(pair[1], value));

    [Fact]
    public async Task UnhandledExceptionHandler_TracksExceptionAndWritesGeneric500()
    {
        var context = CreateContext();
        var exception = new InvalidOperationException("boom");
        var handler = new UnhandledExceptionHandler(_telemetry.Object);

        var handled = await handler.TryHandleAsync(context, exception, CancellationToken.None);

        handled.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        _telemetry.Verify(t => t.TrackException(exception, It.Is<object?[]>(attributes =>
            HasAttribute(attributes, "http.trace_identifier", "trace-123"))), Times.Once);

        var body = await ReadBodyAsync(context);
        body.GetProperty("type").GetString().Should().Be(ErrorTypes.Unexpected);
        body.GetProperty("instance").GetString().Should().Be("trace-123");
    }

    [Fact]
    public async Task BoltonCupExceptionHandler_TracksDomainExceptionAsHandled()
    {
        var context = CreateContext();
        var exception = new EntityNotFoundException("Draft", 42);
        var handler = new BoltonCupExceptionHandler(_telemetry.Object);

        var handled = await handler.TryHandleAsync(context, exception, CancellationToken.None);

        handled.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        _telemetry.Verify(t => t.TrackHandledException(exception, It.IsAny<object?[]>()), Times.Once);
        _telemetry.Verify(t => t.TrackException(It.IsAny<Exception>(), It.IsAny<object?[]>()), Times.Never);

        var body = await ReadBodyAsync(context);
        body.GetProperty("type").GetString().Should().Be(ErrorTypes.NotFound);
    }

    [Fact]
    public async Task BoltonCupExceptionHandler_IgnoresUnmappedException()
    {
        var context = CreateContext();
        var handler = new BoltonCupExceptionHandler(_telemetry.Object);

        var handled = await handler.TryHandleAsync(context, new InvalidCastException(), CancellationToken.None);

        handled.Should().BeFalse();
        _telemetry.VerifyNoOtherCalls();
    }
}
