using Microsoft.AspNetCore.Diagnostics;

namespace BoltonCup.WebAPI.Errors;

public sealed class BoltonCupExceptionHandler(ILogger<BoltonCupExceptionHandler> _logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (BoltonCupExceptionMapper.GetProblemDetails(exception) is not { } problem)
            return false;

        _logger.LogWarning(exception, "Bolton Cup exception: {Type}", exception.GetType().Name);

        context.Response.StatusCode = problem.Status ?? StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsJsonAsync(problem, cancellationToken);
        return true;
    }
}