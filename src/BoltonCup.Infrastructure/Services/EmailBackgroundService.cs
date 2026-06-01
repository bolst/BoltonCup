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
                await ProcessEmailAsync(payload, razorEngine, transport, stoppingToken);
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

    private async Task ProcessEmailAsync(EmailPayload payload, IRazorLightEngine razor, IEmailTransport transport, CancellationToken token)
    {
        try
        {
            _logger.LogInformation("Processing email for {Email}", payload.Email);

            var htmlMessage = await razor.CompileRenderAsync(payload.TemplateName, payload.Model);
            var textMessage = new Html2Markdown.Converter().Convert(htmlMessage);

            await transport.SendAsync(payload.Email, payload.Subject, htmlMessage, textMessage, token);

            _logger.LogInformation("Successfully sent email to {Email}", payload.Email);
        }
        catch (Exception ex)
        {
            // (When we eventually need automatic retries on failure, this is where Hangfire comes in).
            _logger.LogError(ex, "Failed to send email to {Email}", payload.Email);
        }
    }
}
