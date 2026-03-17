using System.Threading.Channels;

namespace BoltonCup.Infrastructure.Services;

public interface IEmailQueue
{
    ValueTask EnqueueAsync(EmailPayload payload);
    ValueTask<EmailPayload> DequeueAsync(CancellationToken cancellationToken);
}

public class EmailQueue : IEmailQueue
{
    private readonly Channel<EmailPayload> _queue;

    public EmailQueue()
    {
        // Bounded means it will hold up to 10,000 emails in memory at once.
        // If it gets full, it prevents the server from running out of RAM.
        var options = new BoundedChannelOptions(10000) { FullMode = BoundedChannelFullMode.Wait };
        _queue = Channel.CreateBounded<EmailPayload>(options);
    }

    public async ValueTask EnqueueAsync(EmailPayload payload) =>
        await _queue.Writer.WriteAsync(payload);

    public async ValueTask<EmailPayload> DequeueAsync(CancellationToken cancellationToken) =>
        await _queue.Reader.ReadAsync(cancellationToken);
}

public sealed record EmailPayload(string Email, string Subject, string TemplateName, object Model);