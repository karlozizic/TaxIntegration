using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using TaxIntegration.Api.Infrastructure;
using TaxIntegration.Api.Models;
using TaxIntegration.Api.Repositories;

namespace TaxIntegration.Api.Controllers;

[ApiController]
[Route("erp/orders")]
public class OrdersController : ControllerBase
{
    private readonly OrderRepository _orders;
    private readonly IntegrationEventRepository _events;
    private readonly TaxCalculationRepository _taxCalcs;
    private readonly IdempotencyRepository _idempotency;
    private readonly EventQueue _queue;

    public OrdersController(OrderRepository orders, IntegrationEventRepository events, TaxCalculationRepository taxCalcs, IdempotencyRepository idempotency, EventQueue queue)
    {
        _orders = orders;
        _events = events;
        _taxCalcs = taxCalcs;
        _idempotency = idempotency;
        _queue = queue;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderRequest request)
    {
        var idempotencyKey = Request.Headers["Idempotency-Key"].FirstOrDefault();

        if (idempotencyKey is not null)
        {
            var cached = await _idempotency.GetByKey(idempotencyKey);
            if (cached is not null)
                return new ContentResult
                {
                    Content = cached.ResponseBody,
                    ContentType = "application/json",
                    StatusCode = cached.ResponseStatus
                };
        }

        var existing = await _orders.GetByExternalId(request.ExternalOrderId);
        if (existing is not null)
            return Conflict(new { error = "Order with this externalOrderId already exists", id = existing.Id });

        var orderId = await _orders.Create(request);
        var eventId = await _events.Create(orderId, "TaxRequested");
        _queue.Enqueue(eventId);

        var responseBody = JsonSerializer.Serialize(new { id = orderId, status = "PendingTax" });

        if (idempotencyKey is not null)
            await _idempotency.Save(idempotencyKey, orderId, responseBody, 202);

        return new ContentResult
        {
            Content = responseBody,
            ContentType = "application/json",
            StatusCode = 202
        };
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var order = await _orders.GetById(id);
        if (order is null)
            return NotFound();
        return Ok(order);
    }

    [HttpGet("{id:guid}/tax")]
    public async Task<IActionResult> GetTax(Guid id)
    {
        var order = await _orders.GetById(id);
        if (order is null)
            return NotFound();

        var tax = await _taxCalcs.GetByOrderId(id);
        if (tax is null)
            return NotFound(new { error = "Tax calculation not available", status = order.Status });

        return Ok(tax);
    }
}
