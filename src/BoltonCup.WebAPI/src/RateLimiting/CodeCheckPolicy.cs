namespace BoltonCup.WebAPI.RateLimiting;

/// <summary>Rate-limiter policy that allows 10 requests per minute per IP and endpoint for code-check endpoints.</summary>
public class CodeCheckPolicy() : FixedWindowIpPolicy(PermitLimit, partitionByPath: true)
{
    /// <summary>Requests allowed per IP and endpoint in each one-minute window.</summary>
    public const int PermitLimit = 10;
}