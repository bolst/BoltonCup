using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace BoltonCup.WebAPI.RateLimiting;

public class StrictEmailCheckPolicy : IRateLimiterPolicy<string>
{
    public Func<OnRejectedContext, CancellationToken, ValueTask>? OnRejected { get; } = (context, cancellationToken) =>
        RateLimitResponder.WriteResponseAsync(context, cancellationToken: cancellationToken);

    public RateLimitPartition<string> GetPartition(HttpContext context)
    {
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "anon";
        
        return RateLimitPartition.GetFixedWindowLimiter(ip, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 5,
            Window = TimeSpan.FromMinutes(1),
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = 0
        });
    }
}