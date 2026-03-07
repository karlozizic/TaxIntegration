using Dapper;
using TaxIntegration.Api.Infrastructure;
using TaxIntegration.Api.Models;

namespace TaxIntegration.Api.Repositories;

public class IntegrationEventRepository
{
    private readonly DbConnectionFactory _db;

    public IntegrationEventRepository(DbConnectionFactory db) => _db = db;

    public async Task<Guid> Create(Guid orderId, string eventType)
    {
        await using var conn = _db.Create();
        var id = Guid.NewGuid();
        await conn.ExecuteAsync("""
            INSERT INTO integration_events (id, order_id, event_type, status)
            VALUES (@Id, @OrderId, @EventType, 'Pending')
            """, new { Id = id, OrderId = orderId, EventType = eventType });
        return id;
    }

    public async Task<IntegrationEvent?> GetById(Guid id)
    {
        await using var conn = _db.Create();
        return await conn.QuerySingleOrDefaultAsync<IntegrationEvent>(
            "SELECT * FROM integration_events WHERE id = @Id", new { Id = id });
    }

    public async Task MarkProcessing(Guid id)
    {
        await using var conn = _db.Create();
        await conn.ExecuteAsync("""
            UPDATE integration_events
            SET status = 'Processing', last_attempt_at = now()
            WHERE id = @Id
            """, new { Id = id });
    }

    public async Task MarkDone(Guid id)
    {
        await using var conn = _db.Create();
        await conn.ExecuteAsync(
            "UPDATE integration_events SET status = 'Done' WHERE id = @Id",
            new { Id = id });
    }

    public async Task IncrementRetry(Guid id, string errorMessage)
    {
        await using var conn = _db.Create();
        await conn.ExecuteAsync("""
            UPDATE integration_events
            SET status = 'Pending', retry_count = retry_count + 1,
                error_message = @ErrorMessage, last_attempt_at = now()
            WHERE id = @Id
            """, new { Id = id, ErrorMessage = errorMessage });
    }

    public async Task<IEnumerable<IntegrationEvent>> GetPendingOrProcessing()
    {
        await using var conn = _db.Create();
        return await conn.QueryAsync<IntegrationEvent>(
            "SELECT * FROM integration_events WHERE status IN ('Pending', 'Processing')");
    }

    public async Task MarkFailed(Guid id, string errorMessage)
    {
        await using var conn = _db.Create();
        await conn.ExecuteAsync("""
            UPDATE integration_events
            SET status = 'Failed', error_message = @ErrorMessage, last_attempt_at = now()
            WHERE id = @Id
            """, new { Id = id, ErrorMessage = errorMessage });
    }
}
