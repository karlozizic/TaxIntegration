using System.Text.Json;

namespace TaxIntegration.Api.Services;

public class TaxApiClient
{
    private readonly IHttpClientFactory _factory;

    private static readonly JsonSerializerOptions SnakeCase = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    public TaxApiClient(IHttpClientFactory factory) => _factory = factory;

    public async Task<TaxApiResult> Calculate(
        string country, long amount, string currency, string idempotencyKey,
        CancellationToken ct = default)
    {
        var http = _factory.CreateClient("TaxApi");
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/tax/calculate")
        {
            Content = JsonContent.Create(new { country, amount, currency })
        };
        request.Headers.Add("Idempotency-Key", idempotencyKey);

        var response = await http.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<TaxApiResult>(SnakeCase, ct);
        return result!;
    }
}

public record TaxApiResult(string ReferenceId, decimal TaxRate, long TaxAmount);
