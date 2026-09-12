namespace BoltonCup.Core;

public interface IEmailLogService
{
    Task LogAsync(string recipient, string subject, string templateName, bool succeeded, string? error, Guid? broadcastId, CancellationToken cancellationToken = default);
}
