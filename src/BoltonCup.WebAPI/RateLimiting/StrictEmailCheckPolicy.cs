using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace BoltonCup.WebAPI.RateLimiting;

/// <summary>Rate-limiter policy that allows 5 requests per minute per IP for sensitive email-check endpoints.</summary>
public class StrictEmailCheckPolicy : IRateLimiterPolicy<string>
{
    /// <summary>Gets the callback invoked when a request is rejected due to the rate limit being exceeded.</summary>
    public Func<OnRejectedContext, CancellationToken, ValueTask>? OnRejected { get; } = (context, cancellationToken) =>
        RateLimitResponder.WriteResponseAsync(context, cancellationToken: cancellationToken);

    /// <summary>Returns the fixed-window rate limit partition keyed by the caller's IP address.</summary>
    public RateLimitPartition<string> GetPartition(HttpContext context)
    {
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "anon";

        return RateLimitPartition.GetFixedWindowLimiter(ip, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 5, Window = TimeSpan.FromMinutes(1), QueueProcessingOrder = QueueProcessingOrder.OldestFirst, QueueLimit = 0
        });
    }
}