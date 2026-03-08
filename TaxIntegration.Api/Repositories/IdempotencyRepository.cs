using Dapper;
using TaxIntegration.Api.Infrastructure;
using TaxIntegration.Api.Models;

namespace TaxIntegration.Api.Repositories;

public class IdempotencyRepository
{
    private readonly DbConnectionFactory _db;

    public IdempotencyRepository(DbConnectionFactory db) => _db = db;

    public async Task<IdempotencyRecord?> GetByKey(string key)
    {
        await using var conn = _db.Create();
        return await conn.QuerySingleOrDefaultAsync<IdempotencyRecord>(
            "SELECT * FROM idempotency_keys WHERE key = @Key", new { Key = key });
    }

    public async Task Save(string key, Guid orderId, string responseBody, int responseStatus)
    {
        await using var conn = _db.Create();
        await conn.ExecuteAsync("""
            INSERT INTO idempotency_keys (key, order_id, response_body, response_status)
            VALUES (@Key, @OrderId, @ResponseBody::jsonb, @ResponseStatus)
            """, new { Key = key, OrderId = orderId, ResponseBody = responseBody, ResponseStatus = responseStatus });
    }
}
