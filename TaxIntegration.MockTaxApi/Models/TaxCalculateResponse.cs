namespace TaxIntegration.MockTaxApi.Models;

public record TaxCalculateResponse(string ReferenceId, decimal TaxRate, long TaxAmount);
