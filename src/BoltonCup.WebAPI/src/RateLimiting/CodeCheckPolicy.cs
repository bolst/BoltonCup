using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace BoltonCup.WebAPI.RateLimiting;

/// <summary>Rate-limiter policy that allows 10 requests per minute per IP and endpoint for code-check endpoints.</summary>
public class CodeCheckPolicy : IRateLimiterPolicy<string>
{
    /// <summary>Requests allowed per IP and endpoint in each one-minute window.</summary>
    public const int PermitLimit = 10;

    /// <summary>Gets the callback invoked when a request is rejected due to the rate limit being exceeded.</summary>
    public Func<OnRejectedContext, CancellationToken, ValueTask>? OnRejected { get; } = (context, cancellationToken) =>
        RateLimitResponder.WriteResponseAsync(context, cancellationToken: cancellationToken);

    /// <summary>Returns the fixed-window rate limit partition keyed by the caller's IP address and the request path.</summary>
    public RateLimitPartition<string> GetPartition(HttpContext context)
    {
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "anon";
        var path = context.Request.Path.Value?.TrimEnd('/').ToLowerInvariant();
        var partitionKey = $"{ip}:{path}";

        return RateLimitPartition.GetFixedWindowLimiter(partitionKey, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = PermitLimit,
            Window = TimeSpan.FromMinutes(1),
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = 0
        });
    }
}