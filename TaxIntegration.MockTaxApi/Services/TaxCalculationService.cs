using TaxIntegration.MockTaxApi.Models;

namespace TaxIntegration.MockTaxApi.Services;

public class TaxCalculationService
{
    private static readonly Dictionary<string, decimal> TaxRates = new(StringComparer.OrdinalIgnoreCase)
    {
        ["HR"] = 25.00m,
        ["DE"] = 19.00m
    };

    private const decimal DefaultTaxRate = 20.00m;

    public TaxCalculateResponse Calculate(string country, long amount)
    {
        var taxRate = TaxRates.GetValueOrDefault(country, DefaultTaxRate);
        var taxAmount = (long)Math.Round(amount * taxRate / 100m);
        var referenceId = $"TAX-{Guid.NewGuid():N}".ToUpper();

        return new TaxCalculateResponse(referenceId, taxRate, taxAmount);
    }
}
