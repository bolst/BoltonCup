using BoltonCup.Core;
using BoltonCup.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RazorLight;

namespace BoltonCup.Infrastructure.Services;

public class EmailBackgroundService : BackgroundService
{
    private readonly IEmailQueue _queue;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EmailBackgroundService> _logger;

    public EmailBackgroundService(IEmailQueue queue, IServiceProvider serviceProvider, ILogger<EmailBackgroundService> logger)
    {
        _queue = queue;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Email Background Service is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Waits quietly until an email is dropped into the queue
                var payload = await _queue.DequeueAsync(stoppingToken);

                using var scope = _serviceProvider.CreateScope();
                var razorEngine = scope.ServiceProvider.GetRequiredService<IRazorLightEngine>();
                var transport = scope.ServiceProvider.GetRequiredService<IEmailTransport>();
                var dbFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<BoltonCupDbContext>>();
                await ProcessEmailAsync(payload, razorEngine, transport, dbFactory, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "A fatal error occurred in the email background service.");
            }
        }
    }

    private async Task ProcessEmailAsync(EmailPayload payload, IRazorLightEngine razor, IEmailTransport transport, IDbContextFactory<BoltonCupDbContext> dbFactory, CancellationToken token)
    {
        var succeeded = false;
        string? error = null;
        try
        {
            _logger.LogInformation("Processing email for {Email}", payload.Email);

            var htmlMessage = await razor.CompileRenderAsync(payload.TemplateName, payload.Model);
            var textMessage = new Html2Markdown.Converter().Convert(htmlMessage);

            await transport.SendAsync(payload.Email, payload.Subject, htmlMessage, textMessage, token);

            succeeded = true;
            _logger.LogInformation("Successfully sent email to {Email}", payload.Email);
        }
        catch (Exception ex)
        {
            // (When we eventually need automatic retries on failure, this is where Hangfire comes in).
            error = ex.Message;
            _logger.LogError(ex, "Failed to send email to {Email}", payload.Email);
        }

        await WriteLogAsync(payload, succeeded, error, dbFactory, token);
    }

    private async Task WriteLogAsync(EmailPayload payload, bool succeeded, string? error, IDbContextFactory<BoltonCupDbContext> dbFactory, CancellationToken token)
    {
        try
        {
            await using var db = await dbFactory.CreateDbContextAsync(token);
            db.EmailLogs.Add(new EmailLog
            {
                Recipient = payload.Email,
                Subject = payload.Subject,
                TemplateName = payload.TemplateName,
                Succeeded = succeeded,
                Error = error,
                BroadcastId = payload.BroadcastId,
            });
            await db.SaveChangesAsync(token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write email log for {Email}", payload.Email);
        }
    }
}
