using BoltonCup.Shared;
using FluentAssertions;
using Xunit;

namespace BoltonCup.WebAPI.Tests.Auth;

public class ReturnUrlPolicyTests
{
    const string RootDomain = "boltoncup.ca";
    static readonly Uri BaseUri = new("https://auth.boltoncup.ca/");

    [Theory]
    [InlineData("https://boltoncup.ca/x", "https://boltoncup.ca/x")]
    [InlineData("https://draft.boltoncup.ca/", "https://draft.boltoncup.ca/")]
    [InlineData("https://WWW.BoltonCup.ca/stats?tab=1", "https://www.boltoncup.ca/stats?tab=1")]
    [InlineData("/profile", "https://auth.boltoncup.ca/profile")]
    [InlineData("https://boltoncup.ca:8443/x", "https://boltoncup.ca:8443/x")]
    [InlineData("/search?q=<script>", "https://auth.boltoncup.ca/search?q=%3Cscript%3E")]
    public void Sanitize_AcceptsBoltonCupHttpsUrls(string raw, string expected)
    {
        ReturnUrlPolicy.Sanitize(raw, BaseUri, RootDomain, allowLocalhost: false).Should().Be(expected);
    }

    [Theory]
    [InlineData("http://localhost:5000/profile")]
    [InlineData("https://127.0.0.1:7001/")]
    public void Sanitize_AcceptsLocalhostOnlyInDevelopment(string raw)
    {
        ReturnUrlPolicy.Sanitize(raw, BaseUri, RootDomain, allowLocalhost: true).Should().NotBeNull();
        ReturnUrlPolicy.Sanitize(raw, BaseUri, RootDomain, allowLocalhost: false).Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("javascript:alert(1)")]
    [InlineData("//evil.com")]
    [InlineData("/\\evil.com")]
    [InlineData("https://evil.com")]
    [InlineData("https://boltoncup.ca.evil.com")]
    [InlineData("https://evilboltoncup.ca")]
    [InlineData("https://u@evil.com")]
    [InlineData("https://u@boltoncup.ca")]
    [InlineData("http://boltoncup.ca")]
    [InlineData("http://[")]
    [InlineData("https://boltoncup.ca.")]
    [InlineData("\t//evil.com")]
    [InlineData("http://127.1/")]
    [InlineData("http://[::1]/")]
    [InlineData("https://xn--boltoncup-ca.evil.com")]
    public void Sanitize_RejectsUnsafeUrls(string? raw)
    {
        ReturnUrlPolicy.Sanitize(raw, BaseUri, RootDomain, allowLocalhost: false).Should().BeNull();
    }

    [Theory]
    [InlineData("https:evil.com")]
    [InlineData("/\t/evil.com")]
    [InlineData("/\n/evil.com")]
    [InlineData("%2F%2Fevil.com")]
    [InlineData("/%5Cevil.com")]
    public void Sanitize_KeepsTrickyRelativeUrlsOnOwnHost(string raw)
    {
        var result = ReturnUrlPolicy.Sanitize(raw, BaseUri, RootDomain, allowLocalhost: false);

        new Uri(result!).Host.Should().Be(BaseUri.Host);
        result.Should().NotContainAny("\t", "\n");
    }

    [Fact]
    public void Sanitize_RejectsEverythingWhenRootDomainIsEmpty()
    {
        ReturnUrlPolicy.Sanitize("https://boltoncup.ca/", BaseUri, string.Empty, allowLocalhost: false).Should().BeNull();
    }
}