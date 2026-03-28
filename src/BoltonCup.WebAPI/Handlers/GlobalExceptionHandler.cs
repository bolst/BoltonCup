using Microsoft.AspNetCore.Diagnostics;
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

        BoltonCupErrorResponse? problemDetails;
        if (exception is BoltonCupException apiException)
        {
            problemDetails = new BoltonCupErrorResponse(
                Title: "An error occurred",
                Message: apiException.Message,
                StatusCode: StatusCodes.Status500InternalServerError
            );
            _logger.LogError(exception, 
                "Processed exception in {ControllerName}::{ActionName}: {ProblemDetails}", 
                controllerName, actionName, problemDetails);
        }
        else
        {
            problemDetails = new BoltonCupErrorResponse(
                Title: "An unexpected error occurred.",
                Message: "Please try again later.",
                StatusCode: StatusCodes.Status500InternalServerError
            );
            _logger.LogError(exception, 
                "Unhandled exception in {ControllerName}::{ActionName}", 
                controllerName, actionName);
        }
        
        httpContext.Response.StatusCode = problemDetails.StatusCode;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true; 
    }
}

public record BoltonCupErrorResponse(string Title, string Message, int StatusCode);