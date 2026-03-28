using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using BoltonCup.Shared;

namespace BoltonCup.WebAPI.Errors;

public sealed class UnhandledExceptionHandler(ILogger<UnhandledExceptionHandler> _logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken ct)
    {
        _logger.LogError(exception, "Unhandled exception");

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Type     = ErrorTypes.Unexpected,
            Title    = "An unexpected error occurred",
            Status   = StatusCodes.Status500InternalServerError,
            Instance = context.TraceIdentifier
        }, ct);

        return true;
    }
}