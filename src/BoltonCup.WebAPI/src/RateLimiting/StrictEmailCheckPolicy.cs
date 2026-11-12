namespace BoltonCup.WebAPI.RateLimiting;

/// <summary>Rate-limiter policy that allows 5 requests per minute per IP for sensitive email-check endpoints.</summary>
public class StrictEmailCheckPolicy() : FixedWindowIpPolicy(PermitLimit)
{
    /// <summary>Requests allowed per IP in each one-minute window.</summary>
    public const int PermitLimit = 5;
}