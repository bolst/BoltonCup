using System.Threading.RateLimiting;

namespace BoltonCup.WebAPI.RateLimiting;

public static class GlobalRateLimiter
{
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
                QueueLimit = 2,
            });
        });
    }
}