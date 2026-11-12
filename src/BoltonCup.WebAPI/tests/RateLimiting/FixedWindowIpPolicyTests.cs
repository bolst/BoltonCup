using System.Net;
using BoltonCup.WebAPI.RateLimiting;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace BoltonCup.WebAPI.Tests.RateLimiting;

public class FixedWindowIpPolicyTests
{
    static HttpContext Request(string ip, string path)
    {
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = IPAddress.Parse(ip);
        context.Request.Path = path;
        return context;
    }

    static int PermitsGranted(FixedWindowIpPolicy policy)
    {
        var partition = policy.GetPartition(Request("203.0.113.7", "/api/auth/login"));
        using var limiter = partition.Factory(partition.PartitionKey);
        var granted = 0;
        while (limiter.AttemptAcquire().IsAcquired)
        {
            granted++;
        }

        return granted;
    }

    [Fact]
    public void IpPolicies_ShareOnePartitionAcrossPaths()
    {
        var policy = new LoginPolicy();

        var first = policy.GetPartition(Request("203.0.113.7", "/api/auth/login"));
        var second = policy.GetPartition(Request("203.0.113.7", "/api/auth/other"));

        second.PartitionKey.Should().Be(first.PartitionKey);
    }

    [Fact]
    public void CodeCheckPolicy_PartitionsByIpAndNormalizedPath()
    {
        var policy = new CodeCheckPolicy();

        var reset = policy.GetPartition(Request("203.0.113.7", "/api/auth/resetPassword"));
        var resetAgain = policy.GetPartition(Request("203.0.113.7", "/API/auth/resetpassword/"));
        var confirm = policy.GetPartition(Request("203.0.113.7", "/api/auth/confirmEmail"));
        var otherCaller = policy.GetPartition(Request("198.51.100.1", "/api/auth/resetPassword"));

        resetAgain.PartitionKey.Should().Be(reset.PartitionKey);
        confirm.PartitionKey.Should().NotBe(reset.PartitionKey);
        otherCaller.PartitionKey.Should().NotBe(reset.PartitionKey);
    }

    [Fact]
    public void EachPolicy_GrantsItsPermitLimit()
    {
        PermitsGranted(new LoginPolicy()).Should().Be(LoginPolicy.PermitLimit);
        PermitsGranted(new CodeCheckPolicy()).Should().Be(CodeCheckPolicy.PermitLimit);
        PermitsGranted(new StrictEmailCheckPolicy()).Should().Be(StrictEmailCheckPolicy.PermitLimit);
    }
}