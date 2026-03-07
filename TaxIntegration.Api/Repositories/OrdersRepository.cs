using Dapper;
using TaxIntegration.Api.Infrastructure;
using TaxIntegration.Api.Models;

namespace TaxIntegration.Api.Repositories;

public class OrderRepository
{
    private readonly DbConnectionFactory _db;

    public OrderRepository(DbConnectionFactory db) => _db = db;

    public async Task<Guid> Create(CreateOrderRequest request)
    {
        await using var conn = _db.Create();
        var id = Guid.NewGuid();

        await conn.ExecuteAsync("""
                                INSERT INTO orders (id, external_order_id, customer_country, total_amount, currency, status)
                                VALUES (@Id, @ExternalOrderId, @CustomerCountry, @TotalAmount, @Currency, 'PendingTax')
                                """, new
        {
            Id = id,
            request.ExternalOrderId,
            request.CustomerCountry,
            request.TotalAmount,
            request.Currency
        });

        return id;
    }

    public async Task<Order?> GetById(Guid id)
    {
        await using var conn = _db.Create();
        return await conn.QuerySingleOrDefaultAsync<Order>(
            "SELECT * FROM orders WHERE id = @Id", new { Id = id });
    }

    public async Task<Order?> GetByExternalId(string externalOrderId)
    {
        await using var conn = _db.Create();
        return await conn.QuerySingleOrDefaultAsync<Order>(
            "SELECT * FROM orders WHERE external_order_id = @ExternalOrderId",
            new { ExternalOrderId = externalOrderId });
    }

    public async Task UpdateStatus(Guid id, string newStatus)
    {
        await using var conn = _db.Create();
        await conn.ExecuteAsync(
            "UPDATE orders SET status = @Status, updated_at = now() WHERE id = @Id",
            new { Status = newStatus, Id = id });
    }
}