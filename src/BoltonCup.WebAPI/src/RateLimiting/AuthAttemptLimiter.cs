using System.Net;
using System.Threading.RateLimiting;

namespace BoltonCup.WebAPI.RateLimiting;

/// <summary>An auth check that guesses a secret, each with its own attempt budget.</summary>
public enum AuthAttemptKind
{
    /// <summary>A password sign-in.</summary>
    Login,

    /// <summary>A password reset code check, whether verifying it or applying the reset.</summary>
    PasswordResetCode,

    /// <summary>An email confirmation code check.</summary>
    EmailConfirmationCode
}

/// <summary>A number of attempts allowed per fixed window.</summary>
/// <param name="PermitLimit">Attempts allowed in each window.</param>
/// <param name="Window">How long each window lasts.</param>
public sealed record AttemptLimit(int PermitLimit, TimeSpan Window);

/// <summary>The budget one caller gets for one email, and the budget every caller shares for that email.</summary>
/// <param name="PerCaller">The budget for one client address and email.</param>
/// <param name="PerEmail">The budget for one email across every client address.</param>
public sealed record AttemptLimits(AttemptLimit PerCaller, AttemptLimit PerEmail);

/// <summary>
/// Caps guesses at a password or emailed code. Each caller gets a tight budget per email, and every caller together
/// gets a looser one, so a stranger exhausts their own budget long before they can block the owner. Keys use the
/// email as typed, so unknown and registered emails answer alike. Counts live in memory and reset on restart.
/// </summary>
public sealed class AuthAttemptLimiter : IDisposable
{
    /// <summary>Limits for <see cref="AuthAttemptKind.Login"/>.</summary>
    public static readonly AttemptLimits LoginLimits = new(
        PerCaller: new AttemptLimit(10, TimeSpan.FromMinutes(15)),
        PerEmail: new AttemptLimit(100, TimeSpan.FromHours(1)));

    /// <summary>Limits for each code check kind.</summary>
    public static readonly AttemptLimits CodeLimits = new(
        PerCaller: new AttemptLimit(10, TimeSpan.FromHours(1)),
        PerEmail: new AttemptLimit(50, TimeSpan.FromHours(1)));

    readonly Dictionary<AuthAttemptKind, (PartitionedRateLimiter<string> PerCaller, PartitionedRateLimiter<string> PerEmail)> _limiters;

    /// <summary>Creates a limiter with empty budgets.</summary>
    public AuthAttemptLimiter()
    {
        _limiters = Enum.GetValues<AuthAttemptKind>().ToDictionary(kind => kind, kind =>
        {
            var limits = LimitsFor(kind);
            return (CreateLimiter(limits.PerCaller), CreateLimiter(limits.PerEmail));
        });
    }

    /// <summary>Returns the limits that apply to <paramref name="kind"/>.</summary>
    public static AttemptLimits LimitsFor(AuthAttemptKind kind) => kind == AuthAttemptKind.Login ? LoginLimits : CodeLimits;

    /// <summary>Records an attempt before the secret is checked. Returns <c>false</c> when either budget is spent.</summary>
    public bool TryAcquire(AuthAttemptKind kind, string email, IPAddress? clientAddress)
    {
        var (perCaller, perEmail) = _limiters[kind];
        var emailKey = email.Trim().ToUpperInvariant();
        using var callerLease = perCaller.AttemptAcquire($"{ClientAddress.PartitionKey(clientAddress)}:{emailKey}");
        if (!callerLease.IsAcquired)
        {
            return false;
        }

        using var emailLease = perEmail.AttemptAcquire(emailKey);
        return emailLease.IsAcquired;
    }

    /// <summary>Disposes the underlying limiters.</summary>
    public void Dispose()
    {
        foreach (var (perCaller, perEmail) in _limiters.Values)
        {
            perCaller.Dispose();
            perEmail.Dispose();
        }
    }

    static PartitionedRateLimiter<string> CreateLimiter(AttemptLimit limit) =>
        PartitionedRateLimiter.Create<string, string>(key => RateLimitPartition.GetFixedWindowLimiter(
            key,
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = limit.PermitLimit,
                Window = limit.Window,
                QueueLimit = 0
            }));
}