using Microsoft.AspNetCore.Diagnostics;
using BoltonCup.Shared;

namespace BoltonCup.WebAPI.Errors;

// we shall handle the unhandled

public sealed class UnhandledExceptionHandler(
    ILogger<UnhandledExceptionHandler> _logger,
    IHub _sentryHub
) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Unhandled exception");

        var problemDetails = new BoltonCupProblemDetails
        {
            Type = ErrorTypes.Unexpected,
            Title = "An unexpected error occurred",
            Status = StatusCodes.Status500InternalServerError,
            Instance = context.TraceIdentifier
        };
        
        // log to sentry
        _sentryHub.CaptureException(exception, scope =>
        {
            scope.SetTag("ErrorType", problemDetails.Type);
            scope.SetExtra("TraceIdentifier", context.TraceIdentifier);
        });

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}