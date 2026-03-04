using TaxIntegration.MockTaxApi.Endpoints;
using TaxIntegration.MockTaxApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(o =>
    o.SerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.SnakeCaseLower);

builder.Services.AddSingleton<IdempotencyStore>();
builder.Services.AddSingleton<TaxCalculationService>();

var app = builder.Build();

app.MapTaxEndpoints();

app.Run();
