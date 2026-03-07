using System.Threading.Channels;

namespace TaxIntegration.Api.Infrastructure;

public class EventQueue
{
    private readonly Channel<Guid> _channel = Channel.CreateUnbounded<Guid>();

    public void Enqueue(Guid eventId) => _channel.Writer.TryWrite(eventId);

    public IAsyncEnumerable<Guid> ReadAllAsync(CancellationToken ct) => _channel.Reader.ReadAllAsync(ct);
}
