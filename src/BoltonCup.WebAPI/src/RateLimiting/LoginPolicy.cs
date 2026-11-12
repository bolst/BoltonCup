namespace BoltonCup.WebAPI.RateLimiting;

/// <summary>Rate-limiter policy that allows 20 login requests per minute per IP.</summary>
public class LoginPolicy() : FixedWindowIpPolicy(PermitLimit)
{
    /// <summary>Login requests allowed per IP in each one-minute window.</summary>
    public const int PermitLimit = 20;
}