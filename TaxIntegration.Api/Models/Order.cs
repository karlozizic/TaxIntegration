namespace TaxIntegration.Api.Models;

public class Order
{
    public Guid Id { get; set; }
    public string ExternalOrderId { get; set; } = "";
    public string CustomerCountry { get; set; } = "";
    public long TotalAmount { get; set; }
    public string Currency { get; set; } = "EUR";
    public string Status { get; set; } = "PendingTax";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}