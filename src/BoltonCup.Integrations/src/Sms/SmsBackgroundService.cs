using BoltonCup.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BoltonCup.Integrations.Sms;

public class SmsBackgroundService : BackgroundService
{
    readonly ISmsQueue _queue;
    readonly IServiceProvider _serviceProvider;
    readonly ILogger<SmsBackgroundService> _logger;

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
                var smsLogService = scope.ServiceProvider.GetRequiredService<ISmsLogService>();
                await ProcessSmsAsync(payload, transport, smsLogService, stoppingToken);
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

    async Task ProcessSmsAsync(SmsPayload payload, ISmsTransport transport, ISmsLogService smsLogService, CancellationToken token)
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

        await WriteLogAsync(payload, succeeded, error, smsLogService, token);
    }

    async Task WriteLogAsync(SmsPayload payload, bool succeeded, string? error, ISmsLogService smsLogService, CancellationToken token)
    {
        try
        {
            await smsLogService.LogAsync(payload.ToPhoneNumber, payload.Body, succeeded, error, token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write SMS log for {Phone}", payload.ToPhoneNumber);
        }
    }
}