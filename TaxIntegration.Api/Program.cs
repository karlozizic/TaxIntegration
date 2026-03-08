using TaxIntegration.Api.Infrastructure;
using TaxIntegration.Api.Repositories;
using TaxIntegration.Api.Services;
using TaxIntegration.Api.Workers;

var builder = WebApplication.CreateBuilder(args);

Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

builder.Services.AddOpenApi();
builder.Services.AddControllers();

builder.Services.AddSingleton<DbConnectionFactory>();
builder.Services.AddSingleton<EventQueue>();

builder.Services.AddScoped<OrderRepository>();
builder.Services.AddScoped<IntegrationEventRepository>();
builder.Services.AddScoped<TaxCalculationRepository>();
builder.Services.AddScoped<IdempotencyRepository>();

builder.Services.AddHttpClient("TaxApi", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["TaxApi:BaseUrl"]!);
    client.Timeout = TimeSpan.FromSeconds(10);
});
builder.Services.AddSingleton<TaxApiClient>();

builder.Services.AddHostedService<RecoveryWorker>();
builder.Services.AddHostedService<TaxCalculationWorker>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseExceptionHandler(errApp => errApp.Run(async ctx =>
{
    ctx.Response.StatusCode = 500;
    ctx.Response.ContentType = "application/json";
    await ctx.Response.WriteAsJsonAsync(new { error = "An unexpected error occurred" });
}));

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
