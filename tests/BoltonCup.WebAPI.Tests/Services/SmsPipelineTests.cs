using System.Diagnostics;
using BoltonCup.Infrastructure.Data;
using BoltonCup.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace BoltonCup.WebAPI.Tests.Services;

public class SmsPipelineTests
{
    private const string ToNumber = "+15555550123";
    private const string Body = "hello from the trade hub";

    private static ServiceProvider BuildProvider(string dbName, ISmsTransport transport)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContextFactory<BoltonCupDbContext>(o => o
            .UseInMemoryDatabase(dbName)
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning)));
        services.AddSingleton(transport);
        return services.BuildServiceProvider();
    }

    private static async Task WaitForAsync(Func<Task<bool>> condition, int timeoutMs = 3000)
    {
        var sw = Stopwatch.StartNew();
        while (sw.ElapsedMilliseconds < timeoutMs)
        {
            if (await condition())
                return;
            await Task.Delay(25);
        }
        throw new TimeoutException("Condition was not met within the timeout.");
    }

    [Fact]
    public async Task SendAsync_DrainsQueue_InvokesTransport_AndWritesSuccessLog()
    {
        var dbName = $"sms-{Guid.NewGuid()}";
        var transport = new Mock<ISmsTransport>();
        await using var provider = BuildProvider(dbName, transport.Object);

        var queue = new SmsQueue();
        var sender = new SmsSender(queue);
        var background = new SmsBackgroundService(queue, provider, provider.GetRequiredService<ILogger<SmsBackgroundService>>());

        await sender.SendAsync(ToNumber, Body);
        await background.StartAsync(CancellationToken.None);

        var factory = provider.GetRequiredService<IDbContextFactory<BoltonCupDbContext>>();
        await WaitForAsync(async () =>
        {
            await using var db = await factory.CreateDbContextAsync();
            return await db.SmsLogs.AnyAsync();
        });

        await background.StopAsync(CancellationToken.None);

        transport.Verify(t => t.SendAsync(ToNumber, Body, It.IsAny<CancellationToken>()), Times.Once);

        await using var assertDb = await factory.CreateDbContextAsync();
        var log = await assertDb.SmsLogs.SingleAsync();
        log.Recipient.Should().Be(ToNumber);
        log.Body.Should().Be(Body);
        log.Succeeded.Should().BeTrue();
        log.Error.Should().BeNull();
    }

    [Fact]
    public async Task SendAsync_WhenTransportThrows_WritesFailureLog()
    {
        var dbName = $"sms-{Guid.NewGuid()}";
        var transport = new Mock<ISmsTransport>();
        transport
            .Setup(t => t.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("twilio rejected the number"));
        await using var provider = BuildProvider(dbName, transport.Object);

        var queue = new SmsQueue();
        var sender = new SmsSender(queue);
        var background = new SmsBackgroundService(queue, provider, provider.GetRequiredService<ILogger<SmsBackgroundService>>());

        await sender.SendAsync(ToNumber, Body);
        await background.StartAsync(CancellationToken.None);

        var factory = provider.GetRequiredService<IDbContextFactory<BoltonCupDbContext>>();
        await WaitForAsync(async () =>
        {
            await using var db = await factory.CreateDbContextAsync();
            return await db.SmsLogs.AnyAsync();
        });

        await background.StopAsync(CancellationToken.None);

        await using var assertDb = await factory.CreateDbContextAsync();
        var log = await assertDb.SmsLogs.SingleAsync();
        log.Succeeded.Should().BeFalse();
        log.Error.Should().Be("twilio rejected the number");
    }
}
