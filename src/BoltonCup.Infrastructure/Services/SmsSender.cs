namespace BoltonCup.Infrastructure.Services;

public interface ISmsSender
{
    /// <summary>Enqueues a text message for background delivery.</summary>
    Task SendAsync(string toPhoneNumber, string message, CancellationToken cancellationToken = default);
}

public sealed class SmsSender(ISmsQueue _queue) : ISmsSender
{
    public async Task SendAsync(string toPhoneNumber, string message, CancellationToken cancellationToken = default) =>
        await _queue.EnqueueAsync(new SmsPayload(toPhoneNumber, message));
}
