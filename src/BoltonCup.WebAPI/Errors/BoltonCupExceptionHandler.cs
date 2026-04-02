using Microsoft.AspNetCore.Diagnostics;

namespace BoltonCup.WebAPI.Errors;

public sealed class BoltonCupExceptionHandler(
    ILogger<BoltonCupExceptionHandler> _logger,
    IHub _sentryHub
) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (BoltonCupExceptionMappings.GetProblemDetails(exception) is not { } problem)
            return false;

        _logger.LogWarning(exception, "Bolton Cup exception: {Type}", exception.GetType().Name);
        
        // log to sentry
        _sentryHub.CaptureException(exception, scope =>
        {
            scope.Level = SentryLevel.Warning;
            scope.SetExtra("Problem.Type", problem.Type);
            scope.SetExtra("Problem.Status", problem.Status);
            scope.SetExtra("Problem.Title", problem.Title);
            scope.SetExtra("Problem.Detail", problem.Detail);
        });

        context.Response.StatusCode = problem.Status ?? StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsJsonAsync(problem, cancellationToken);
        return true;
    }
}