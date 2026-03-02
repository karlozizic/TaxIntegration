namespace TaxIntegration.Api.Models;

public class CreateOrderRequest
{
    public string ExternalOrderId { get; set; } = "";
    public string CustomerCountry { get; set; } = "";
    public long TotalAmount { get; set; }
    public string Currency { get; set; } = "EUR";
}