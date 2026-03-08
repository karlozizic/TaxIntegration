using TaxIntegration.Api.Infrastructure;
using TaxIntegration.Api.Repositories;

namespace TaxIntegration.Api.Workers;

public class RecoveryWorker : IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly EventQueue _queue;
    private readonly ILogger<RecoveryWorker> _logger;

    public RecoveryWorker(IServiceScopeFactory scopeFactory, EventQueue queue, ILogger<RecoveryWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _queue = queue;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var events = scope.ServiceProvider.GetRequiredService<IntegrationEventRepository>();

        var pending = await events.GetPendingOrProcessing();

        var count = 0;
        foreach (var evt in pending)
        {
            _queue.Enqueue(evt.Id);
            count++;
        }

        if (count > 0)
            _logger.LogWarning("Recovery: re-enqueued {Count} unfinished events", count);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
