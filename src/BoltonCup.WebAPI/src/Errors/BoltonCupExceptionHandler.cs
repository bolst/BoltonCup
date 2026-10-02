using Microsoft.AspNetCore.Diagnostics;
using BoltonCup.Core;

namespace BoltonCup.WebAPI.Errors;

/// <summary>Handles known BoltonCup domain exceptions and converts them to structured problem-detail responses.</summary>
public sealed class BoltonCupExceptionHandler(ITelemetry _telemetry) : IExceptionHandler
{
    /// <inheritdoc/>
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (BoltonCupExceptionMappings.GetProblemDetails(exception) is not { } problem)
        {
            return false;
        }

        _telemetry.TrackHandledException(exception,
            "problem.type", problem.Type,
            "problem.status", problem.Status,
            "problem.title", problem.Title);

        context.Response.StatusCode = problem.Status ?? StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsJsonAsync(problem, cancellationToken);
        return true;
    }
}
