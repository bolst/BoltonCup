using System.Net;
using BoltonCup.WebAPI.RateLimiting;
using FluentAssertions;
using Xunit;

namespace BoltonCup.WebAPI.Tests.RateLimiting;

public class ClientAddressTests
{
    [Theory]
    [InlineData("203.0.113.7", "203.0.113.7")]
    [InlineData("::ffff:203.0.113.7", "203.0.113.7")]
    [InlineData("2001:db8:1:1:aaaa:bbbb:cccc:dddd", "2001:db8:1:1::/64")]
    [InlineData("2001:db8:1:1::1", "2001:db8:1:1::/64")]
    public void PartitionKey_KeysIPv4ByAddressAndIPv6BySlash64(string address, string expected) =>
        ClientAddress.PartitionKey(IPAddress.Parse(address)).Should().Be(expected);

    [Fact]
    public void PartitionKey_NoAddress_IsAnonymous() => ClientAddress.PartitionKey(null).Should().Be("anon");
}