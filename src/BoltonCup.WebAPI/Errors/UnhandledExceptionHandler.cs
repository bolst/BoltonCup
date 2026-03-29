using Microsoft.AspNetCore.Diagnostics;
using BoltonCup.Shared;

namespace BoltonCup.WebAPI.Errors;

// we shall handle the unhandled

public sealed class UnhandledExceptionHandler(ILogger<UnhandledExceptionHandler> _logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Unhandled exception");

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsJsonAsync(new BoltonCupProblemDetails
        {
            Type     = ErrorTypes.Unexpected,
            Title    = "An unexpected error occurred",
            Status   = StatusCodes.Status500InternalServerError,
            Instance = context.TraceIdentifier
        }, cancellationToken);

        return true;
    }
}