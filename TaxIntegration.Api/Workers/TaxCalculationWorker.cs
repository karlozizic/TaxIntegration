using TaxIntegration.Api.Infrastructure;
using TaxIntegration.Api.Repositories;
using TaxIntegration.Api.Services;

namespace TaxIntegration.Api.Workers;

public class TaxCalculationWorker : BackgroundService
{
    private readonly EventQueue _queue;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly TaxApiClient _taxApiClient;
    private readonly ILogger<TaxCalculationWorker> _logger;

    public TaxCalculationWorker(EventQueue queue, IServiceScopeFactory scopeFactory, TaxApiClient taxApiClient, ILogger<TaxCalculationWorker> logger)
    {
        _queue = queue;
        _scopeFactory = scopeFactory;
        _taxApiClient = taxApiClient;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var eventId in _queue.ReadAllAsync(stoppingToken))
        {
            try
            {
                await ProcessEvent(eventId, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process event {EventId}", eventId);
            }
        }
    }

    private async Task ProcessEvent(Guid eventId, CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var events = scope.ServiceProvider.GetRequiredService<IntegrationEventRepository>();
        var orders = scope.ServiceProvider.GetRequiredService<OrderRepository>();
        var taxCalcs = scope.ServiceProvider.GetRequiredService<TaxCalculationRepository>();

        var evt = await events.GetById(eventId);
        var order = await orders.GetById(evt!.OrderId);

        await events.MarkProcessing(eventId);

        try
        {
            var result = await _taxApiClient.Calculate(order!.CustomerCountry, order.TotalAmount, order.Currency, eventId.ToString(), ct);

            await taxCalcs.Create(order.Id, result.TaxRate, result.TaxAmount, result.ReferenceId);
            await orders.UpdateStatus(order.Id, "TaxCalculated");
            await events.MarkDone(eventId);

            _logger.LogInformation("Tax calculated for order {OrderId}: {Rate}% → {Amount}", order.Id, result.TaxRate, result.TaxAmount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Tax calculation failed for order {OrderId}", order!.Id);
            await events.MarkFailed(eventId, ex.Message);
            await orders.UpdateStatus(order.Id, "Failed");
        }
    }
}
