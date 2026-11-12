using System.Net;
using BoltonCup.WebAPI.RateLimiting;
using FluentAssertions;
using Xunit;

namespace BoltonCup.WebAPI.Tests.RateLimiting;

public class AuthAttemptLimiterTests : IDisposable
{
    const string Email = "player@example.com";

    readonly AuthAttemptLimiter _limiter = new();

    public void Dispose() => _limiter.Dispose();

    static IPAddress Client(int n) => IPAddress.Parse($"203.0.113.{n}");

    int AttemptsAllowed(AuthAttemptKind kind, string email, IPAddress client, int tries)
    {
        var allowed = 0;
        for (var i = 0; i < tries; i++)
        {
            if (_limiter.TryAcquire(kind, email, client))
            {
                allowed++;
            }
        }

        return allowed;
    }

    public static TheoryData<AuthAttemptKind> Kinds => new()
    {
        AuthAttemptKind.Login,
        AuthAttemptKind.PasswordResetCode,
        AuthAttemptKind.EmailConfirmationCode
    };

    [Theory]
    [MemberData(nameof(Kinds))]
    public void OneCaller_GetsItsPerCallerBudget(AuthAttemptKind kind)
    {
        var perCaller = AuthAttemptLimiter.LimitsFor(kind).PerCaller.PermitLimit;

        AttemptsAllowed(kind, Email, Client(1), perCaller + 5).Should().Be(perCaller);
    }

    [Theory]
    [MemberData(nameof(Kinds))]
    public void SpentCaller_DoesNotBlockAnotherCallerOnTheSameEmail(AuthAttemptKind kind)
    {
        var perCaller = AuthAttemptLimiter.LimitsFor(kind).PerCaller.PermitLimit;
        AttemptsAllowed(kind, Email, Client(1), perCaller + 5);

        _limiter.TryAcquire(kind, Email, Client(2)).Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(Kinds))]
    public void ManyCallers_StopAtThePerEmailBudget(AuthAttemptKind kind)
    {
        var limits = AuthAttemptLimiter.LimitsFor(kind);
        var callersToFill = limits.PerEmail.PermitLimit / limits.PerCaller.PermitLimit;

        var allowed = Enumerable.Range(1, callersToFill + 1)
            .Sum(n => AttemptsAllowed(kind, Email, Client(n), limits.PerCaller.PermitLimit));

        allowed.Should().Be(limits.PerEmail.PermitLimit);
        _limiter.TryAcquire(kind, Email, Client(callersToFill + 2)).Should().BeFalse();
    }

    [Fact]
    public void SpentCaller_DoesNotSpendTheEmailBudget()
    {
        var limits = AuthAttemptLimiter.LimitsFor(AuthAttemptKind.Login);

        AttemptsAllowed(AuthAttemptKind.Login, Email, Client(1), limits.PerEmail.PermitLimit * 2);

        var others = Enumerable.Range(2, limits.PerEmail.PermitLimit / limits.PerCaller.PermitLimit - 1)
            .Sum(n => AttemptsAllowed(AuthAttemptKind.Login, Email, Client(n), limits.PerCaller.PermitLimit));
        others.Should().Be(limits.PerEmail.PermitLimit - limits.PerCaller.PermitLimit);
    }

    [Fact]
    public void OtherEmails_HaveTheirOwnBudgets()
    {
        var perCaller = AuthAttemptLimiter.LoginLimits.PerCaller.PermitLimit;
        AttemptsAllowed(AuthAttemptKind.Login, Email, Client(1), perCaller);

        _limiter.TryAcquire(AuthAttemptKind.Login, "other@example.com", Client(1)).Should().BeTrue();
    }

    [Fact]
    public void Kinds_HaveTheirOwnBudgets()
    {
        var perCaller = AuthAttemptLimiter.LoginLimits.PerCaller.PermitLimit;
        AttemptsAllowed(AuthAttemptKind.Login, Email, Client(1), perCaller);

        _limiter.TryAcquire(AuthAttemptKind.PasswordResetCode, Email, Client(1)).Should().BeTrue();
    }

    [Fact]
    public void EmailCaseAndSpacing_ShareOneBudget()
    {
        var perCaller = AuthAttemptLimiter.LoginLimits.PerCaller.PermitLimit;
        AttemptsAllowed(AuthAttemptKind.Login, Email, Client(1), perCaller);

        _limiter.TryAcquire(AuthAttemptKind.Login, "  Player@Example.COM ", Client(1)).Should().BeFalse();
    }

    [Fact]
    public void IPv6Callers_ShareABudgetPerSlash64()
    {
        var perCaller = AuthAttemptLimiter.LoginLimits.PerCaller.PermitLimit;
        AttemptsAllowed(AuthAttemptKind.Login, Email, IPAddress.Parse("2001:db8:1:1::1"), perCaller);

        _limiter.TryAcquire(AuthAttemptKind.Login, Email, IPAddress.Parse("2001:db8:1:1:ffff::2")).Should().BeFalse();
        _limiter.TryAcquire(AuthAttemptKind.Login, Email, IPAddress.Parse("2001:db8:1:2::1")).Should().BeTrue();
    }
}