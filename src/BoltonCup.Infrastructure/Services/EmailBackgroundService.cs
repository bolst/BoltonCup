using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using RazorLight;
using System.Text.RegularExpressions;
using BoltonCup.Infrastructure.Settings;

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
                var smtpSettings = scope.ServiceProvider.GetRequiredService<IOptions<SmtpSettings>>().Value;
                await ProcessEmailAsync(payload, razorEngine, smtpSettings, stoppingToken);
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

    private async Task ProcessEmailAsync(EmailPayload payload, IRazorLightEngine razor, SmtpSettings settings, CancellationToken token)
    {
        try
        {
            _logger.LogInformation("Processing email for {Email}", payload.Email);

            var htmlMessage = await razor.CompileRenderAsync(payload.TemplateName, payload.Model);

            var converter = new Html2Markdown.Converter();
            var textMessage = converter.Convert(htmlMessage);

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(settings.SenderName, settings.SenderEmail));
            message.To.Add(new MailboxAddress("", payload.Email));
            message.Subject = payload.Subject;
            message.Body = new BodyBuilder { HtmlBody = htmlMessage, TextBody = textMessage }.ToMessageBody();

            // Connect and send
            using var client = new SmtpClient { CheckCertificateRevocation = false };
            await client.ConnectAsync(settings.Host, settings.Port, SecureSocketOptions.SslOnConnect, token);
            await client.AuthenticateAsync(settings.Username, settings.Password, token);
            await client.SendAsync(message, token);
            await client.DisconnectAsync(true, token);

            _logger.LogInformation("Successfully sent email to {Email}", payload.Email);
        }
        catch (Exception ex)
        {
            // (When we eventually need automatic retries on failure, this is where Hangfire comes in).
            _logger.LogError(ex, "Failed to send email to {Email}", payload.Email);
        }
    }
}