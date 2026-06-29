using System.Threading.Channels;

namespace BoltonCup.Infrastructure.Services;

public interface ISmsQueue
{
    ValueTask EnqueueAsync(SmsPayload payload);
    ValueTask<SmsPayload> DequeueAsync(CancellationToken cancellationToken);
}

public class SmsQueue : ISmsQueue
{
    private readonly Channel<SmsPayload> _queue;

    public SmsQueue()
    {
        // Bounded means it will hold up to 10,000 messages in memory at once.
        // If it gets full, it prevents the server from running out of RAM.
        var options = new BoundedChannelOptions(10000) { FullMode = BoundedChannelFullMode.Wait };
        _queue = Channel.CreateBounded<SmsPayload>(options);
    }

    public async ValueTask EnqueueAsync(SmsPayload payload) =>
        await _queue.Writer.WriteAsync(payload);

    public async ValueTask<SmsPayload> DequeueAsync(CancellationToken cancellationToken) =>
        await _queue.Reader.ReadAsync(cancellationToken);
}

public sealed record SmsPayload(string ToPhoneNumber, string Body);
