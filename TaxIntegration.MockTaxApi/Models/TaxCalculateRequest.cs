namespace TaxIntegration.MockTaxApi.Models;

public record TaxCalculateRequest(string Country, long Amount, string Currency = "EUR");
