using Microsoft.AspNetCore.Mvc;
using TaxIntegration.Api.Models;

namespace TaxIntegration.Api.Controllers;

[ApiController]
[Route("erp/orders")]
public class OrdersController : ControllerBase
{
    private readonly OrderRepository _orders;

    public OrdersController(OrderRepository orders) => _orders = orders;

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderRequest request)
    {
        var id = await _orders.Create(request);
        return Accepted(new { id, status = "PendingTax" });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var order = await _orders.GetById(id);
        if (order is null) 
            return NotFound();
        
        return Ok(order);
    }
}