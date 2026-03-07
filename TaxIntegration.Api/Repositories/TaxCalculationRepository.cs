using Dapper;
using TaxIntegration.Api.Infrastructure;
using TaxIntegration.Api.Models;

namespace TaxIntegration.Api.Repositories;

public class TaxCalculationRepository
{
    private readonly DbConnectionFactory _db;

    public TaxCalculationRepository(DbConnectionFactory db) => _db = db;

    public async Task Create(Guid orderId, decimal taxRate, long taxAmount, string? reference)
    {
        await using var conn = _db.Create();
        await conn.ExecuteAsync("""
            INSERT INTO tax_calculations (id, order_id, tax_rate, tax_amount, tax_provider_reference)
            VALUES (@Id, @OrderId, @TaxRate, @TaxAmount, @Reference)
            """, new { Id = Guid.NewGuid(), OrderId = orderId, TaxRate = taxRate, TaxAmount = taxAmount, Reference = reference });
    }

    public async Task<TaxCalculation?> GetByOrderId(Guid orderId)
    {
        await using var conn = _db.Create();
        return await conn.QuerySingleOrDefaultAsync<TaxCalculation>(
            "SELECT * FROM tax_calculations WHERE order_id = @OrderId", new { OrderId = orderId });
    }
}
