namespace BoltonCup.Core;

public interface ISmsLogService
{
    Task LogAsync(string recipient, string body, bool succeeded, string? error, CancellationToken cancellationToken = default);
}
