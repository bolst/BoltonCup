using System.Threading.RateLimiting;

namespace BoltonCup.WebAPI.RateLimiting;

/// <summary>Provides a global sliding-window rate limiter partitioned by user identity or IP address.</summary>
public static class GlobalRateLimiter
{
    /// <summary>Creates a partitioned rate limiter that allows 100 requests per minute per user or IP.</summary>
    public static PartitionedRateLimiter<HttpContext> Create()
    {
        return PartitionedRateLimiter.Create<HttpContext, string>(context =>
        {
            string partitionKey = context.User.Identity?.Name ??
                                  context.Connection.RemoteIpAddress?.ToString() ?? 
                                  "anon";
            return RateLimitPartition.GetSlidingWindowLimiter(partitionKey, _ => new SlidingWindowRateLimiterOptions
            {
                PermitLimit = 100, 
                SegmentsPerWindow = 4,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0,
            });
        });
    }
}