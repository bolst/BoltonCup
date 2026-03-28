using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using BoltonCup.Infrastructure.Exceptions;

namespace BoltonCup.WebAPI.Handlers;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> _logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, 
        Exception exception, 
        CancellationToken cancellationToken)
    {
        var controllerName = httpContext.Request.RouteValues["controller"]?.ToString() ?? "Unknown";
        var actionName = httpContext.Request.RouteValues["action"]?.ToString() ?? "Unknown";

        ProblemDetails? problemDetails;
        if (exception is BoltonCupException apiException)
        {
            problemDetails = new ProblemDetails
            {
                Status = apiException.StatusCode,
                Title = "An error occurred",
                Detail = apiException.Message,
            };
            _logger.LogError(exception, 
                "Processed exception in {ControllerName}::{ActionName}: {ProblemDetails}", 
                controllerName, actionName, problemDetails);
        }
        else
        {
            problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "An unexpected error occurred",
                Detail = "Please try again later.",
            };
            _logger.LogError(exception, "Unhandled exception in {ControllerName}::{ActionName}.", controllerName, actionName);
        }
        
        httpContext.Response.StatusCode = problemDetails.Status.Value;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true; 
    }
}
