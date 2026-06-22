using Microsoft.Extensions.Logging;

namespace BoltonCup.Infrastructure.Services;

/// <summary>
/// An <see cref="IEmailTransport"/> that logs messages instead of sending them. Used locally so the full
/// email pipeline (template rendering, email_logs writes) runs without delivering real mail.
/// </summary>
public sealed class LoggingEmailTransport : IEmailTransport
{
    readonly ILogger<LoggingEmailTransport> _logger;

    public LoggingEmailTransport(ILogger<LoggingEmailTransport> logger)
    {
        _logger = logger;
        _logger.LogInformation("LoggingEmailTransport initialized");
    }
    public Task SendAsync(string toEmail, string subject, string htmlBody, string textBody, CancellationToken cancellationToken)
    {
        _logger.LogInformation("[DEV EMAIL — not sent] To: {Email} | Subject: {Subject}\n{TextBody}", toEmail, subject, textBody);
        return Task.CompletedTask;
    }
}
