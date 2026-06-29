using BoltonCup.Core;
using BoltonCup.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BoltonCup.Infrastructure.Services;

public class SmsBackgroundService : BackgroundService
{
    private readonly ISmsQueue _queue;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SmsBackgroundService> _logger;

    public SmsBackgroundService(ISmsQueue queue, IServiceProvider serviceProvider, ILogger<SmsBackgroundService> logger)
    {
        _queue = queue;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("SMS Background Service is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Waits quietly until a message is dropped into the queue
                var payload = await _queue.DequeueAsync(stoppingToken);

                using var scope = _serviceProvider.CreateScope();
                var transport = scope.ServiceProvider.GetRequiredService<ISmsTransport>();
                var dbFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<BoltonCupDbContext>>();
                await ProcessSmsAsync(payload, transport, dbFactory, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "A fatal error occurred in the SMS background service.");
            }
        }
    }

    private async Task ProcessSmsAsync(SmsPayload payload, ISmsTransport transport, IDbContextFactory<BoltonCupDbContext> dbFactory, CancellationToken token)
    {
        var succeeded = false;
        string? error = null;
        try
        {
            _logger.LogInformation("Processing SMS for {Phone}", payload.ToPhoneNumber);

            await transport.SendAsync(payload.ToPhoneNumber, payload.Body, token);

            succeeded = true;
            _logger.LogInformation("Successfully sent SMS to {Phone}", payload.ToPhoneNumber);
        }
        catch (Exception ex)
        {
            error = ex.Message;
            _logger.LogError(ex, "Failed to send SMS to {Phone}", payload.ToPhoneNumber);
        }

        await WriteLogAsync(payload, succeeded, error, dbFactory, token);
    }

    private async Task WriteLogAsync(SmsPayload payload, bool succeeded, string? error, IDbContextFactory<BoltonCupDbContext> dbFactory, CancellationToken token)
    {
        try
        {
            await using var db = await dbFactory.CreateDbContextAsync(token);
            db.SmsLogs.Add(new SmsLog
            {
                Recipient = payload.ToPhoneNumber,
                Body = payload.Body,
                Succeeded = succeeded,
                Error = error,
            });
            await db.SaveChangesAsync(token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write SMS log for {Phone}", payload.ToPhoneNumber);
        }
    }
}
