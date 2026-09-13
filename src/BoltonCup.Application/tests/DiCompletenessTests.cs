using BoltonCup.Core;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BoltonCup.Application.Tests;

// Catches RegisterByConvention gaps (renamed service, unmatched interface) at test time instead of runtime.
public class DiCompletenessTests
{
    static readonly IServiceProvider Provider = BuildServiceProvider();

    static IServiceProvider BuildServiceProvider()
    {
        var builder = WebApplication.CreateBuilder();

        // Placeholder values are enough - Npgsql/S3/Stripe clients connect lazily, not at registration.
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["BoltonCup:ConnectionString"] = "Host=localhost;Database=di-completeness-test",
            ["CloudflareR2:AccountId"] = "test",
            ["CloudflareR2:AccessKey"] = "test",
            ["CloudflareR2:SecretKey"] = "test",
            ["CloudflareR2:BaseUrl"] = "https://example.com/",
            ["Stripe:ApiKey"] = "test",
            ["Resend:Enabled"] = "false",
            ["Twilio:Enabled"] = "false",
        });

        // Mirrors BoltonCup.WebAPI/src/Program.cs's registration order.
        builder.AddBoltonCupApplication().AddBoltonCupAssetUrlResolver();

        return builder.Services.BuildServiceProvider();
    }

    // Suffixes used by RegisterByConvention ("Service") and its manual exceptions ("Validator"/"Generator"/"Queue").
    public static IEnumerable<object[]> CoreDiInterfaces()
    {
        return typeof(IAccountService).Assembly.GetTypes()
            .Where(t => t is { IsInterface: true, Namespace: "BoltonCup.Core", IsGenericTypeDefinition: false })
            .Where(t => t.Name.EndsWith("Service") || t.Name.EndsWith("Validator") || t.Name.EndsWith("Generator") || t.Name.EndsWith("Queue"))
            .Select(t => new object[] { t });
    }

    [Theory]
    [MemberData(nameof(CoreDiInterfaces))]
    public void CoreInterface_ResolvesFromDi(Type coreInterface)
    {
        Provider.GetService(coreInterface).Should().NotBeNull($"{coreInterface.Name} should be registered in DI");
    }

    [Fact]
    public void GenericTagService_ResolvesFromDi()
    {
        Provider.GetService(typeof(ITagService<HighlightTag>)).Should().NotBeNull();
    }
}
