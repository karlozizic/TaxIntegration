namespace TaxIntegration.Api.Models;

public class TaxCalculation
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public decimal TaxRate { get; set; }
    public long TaxAmount { get; set; }
    public string? TaxProviderReference { get; set; }
    public DateTime CreatedAt { get; set; }
}
