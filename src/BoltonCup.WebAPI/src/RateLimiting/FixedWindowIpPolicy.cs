using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace BoltonCup.WebAPI.RateLimiting;

/// <summary>
/// Rate-limiter policy that allows <paramref name="permitLimit"/> requests per one-minute fixed window, keyed by the
/// caller's IP address (see <see cref="ClientAddress"/>) and, when <paramref name="partitionByPath"/> is set, the request path too.
/// </summary>
public abstract class FixedWindowIpPolicy(int permitLimit, bool partitionByPath = false) : IRateLimiterPolicy<string>
{
    /// <summary>Gets the callback invoked when a request is rejected due to the rate limit being exceeded.</summary>
    public Func<OnRejectedContext, CancellationToken, ValueTask>? OnRejected { get; } = (context, cancellationToken) =>
        RateLimitResponder.WriteResponseAsync(context, cancellationToken: cancellationToken);

    /// <summary>Returns the fixed-window rate limit partition for the caller.</summary>
    public RateLimitPartition<string> GetPartition(HttpContext context)
    {
        var partitionKey = ClientAddress.PartitionKey(context.Connection.RemoteIpAddress);
        if (partitionByPath)
        {
            partitionKey += $":{context.Request.Path.Value?.TrimEnd('/').ToLowerInvariant()}";
        }

        return RateLimitPartition.GetFixedWindowLimiter(partitionKey, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = permitLimit,
            Window = TimeSpan.FromMinutes(1),
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = 0
        });
    }
}