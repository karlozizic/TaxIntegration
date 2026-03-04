using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using TaxIntegration.MockTaxApi.Models;
using TaxIntegration.MockTaxApi.Services;

namespace TaxIntegration.MockTaxApi.Endpoints;

public static class TaxEndpoints
{
    private static readonly Random Random = new();
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    public static void MapTaxEndpoints(this WebApplication app)
    {
        app.MapPost("/api/v1/tax/calculate", HandleCalculate);
        app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));
    }

    private static async Task<IResult> HandleCalculate(
        TaxCalculateRequest? req,
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
        HttpContext ctx,
        IdempotencyStore idempotencyStore,
        TaxCalculationService taxService)
    {
        await Task.Delay(Random.Next(100, 2001));

        if (Random.NextDouble() < 0.05)
            return Results.Problem("Internal tax engine error", statusCode: 500);

        if (idempotencyKey is not null && idempotencyStore.TryGetValue(idempotencyKey, out var cached))
        {
            ctx.Response.Headers["X-Idempotent-Replayed"] = "true";
            return Results.Content(cached, "application/json");
        }

        if (req is null || string.IsNullOrWhiteSpace(req.Country) || req.Amount <= 0)
            return Results.BadRequest(new { error = "country and amount are required" });

        var response = taxService.Calculate(req.Country, req.Amount);
        var responseJson = JsonSerializer.Serialize(response, JsonOptions);

        if (idempotencyKey is not null)
            idempotencyStore.TryAdd(idempotencyKey, responseJson);

        return Results.Content(responseJson, "application/json");
    }
}
