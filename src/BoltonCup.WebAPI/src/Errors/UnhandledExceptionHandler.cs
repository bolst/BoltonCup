using Microsoft.AspNetCore.Diagnostics;
using BoltonCup.Core;
using BoltonCup.Shared;

namespace BoltonCup.WebAPI.Errors;

// we shall handle the unhandled
/// <summary>Catches all unhandled exceptions and returns a generic 500 problem-detail response.</summary>
public sealed class UnhandledExceptionHandler(ITelemetry _telemetry) : IExceptionHandler
{
    /// <inheritdoc/>
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var problemDetails = new BoltonCupProblemDetails
        {
            Type = ErrorTypes.Unexpected,
            Title = "An unexpected error occurred",
            Status = StatusCodes.Status500InternalServerError,
            Instance = context.TraceIdentifier
        };

        _telemetry.TrackException(exception, "error.type", problemDetails.Type, "http.trace_identifier", context.TraceIdentifier);

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
