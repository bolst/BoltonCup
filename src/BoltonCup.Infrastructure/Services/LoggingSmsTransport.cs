using Microsoft.Extensions.Logging;

namespace BoltonCup.Infrastructure.Services;

/// <summary>
/// An <see cref="ISmsTransport"/> that logs messages instead of sending them. Used locally so the full
/// SMS pipeline (queue, sms_logs writes) runs without delivering real texts.
/// </summary>
public sealed class LoggingSmsTransport : ISmsTransport
{
    private readonly ILogger<LoggingSmsTransport> _logger;

    public LoggingSmsTransport(ILogger<LoggingSmsTransport> logger)
    {
        _logger = logger;
        _logger.LogInformation("LoggingSmsTransport initialized");
    }

    public Task SendAsync(string toPhoneNumber, string body, CancellationToken cancellationToken)
    {
        _logger.LogInformation("[DEV SMS — not sent] To: {Phone}\n{Body}", toPhoneNumber, body);
        return Task.CompletedTask;
    }
}
