using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using BoltonCup.Shared;
using BoltonCup.WebAPI.Errors;

namespace BoltonCup.WebAPI.RateLimiting;

public static class RateLimitResponder
{
    public static async ValueTask WriteResponseAsync(
        OnRejectedContext context, 
        string title = "Too Many Requests.", 
        string detail = "Rate limit exceeded, please try again later.", 
        string errorType = ErrorTypes.TooManyRequests,
        CancellationToken cancellationToken = default)
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

        var problem = new BoltonCupProblemDetails
        {
            Type = errorType,
            Title = title,
            Status = StatusCodes.Status429TooManyRequests,
            Detail = detail,
            Instance = context.HttpContext.Request.Path
        };

        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            context.HttpContext.Response.Headers.RetryAfter = ((int)retryAfter.TotalSeconds).ToString();
            problem.Extensions["retryAfterSeconds"] = retryAfter.TotalSeconds;
        }

        await context.HttpContext.Response.WriteAsJsonAsync(problem, cancellationToken: cancellationToken);
    }
}