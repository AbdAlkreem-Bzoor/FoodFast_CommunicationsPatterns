using FoodFast.API.Data;
using FoodFast.API.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Channels;

namespace FoodFast.API.Controllers;

using FoodFast.API.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly FoodFastDbContext _database;
    private readonly IHubContext<OrderHub> _orderHub;
    private readonly IHubContext<RestaurantHub> _restaurantHub;

    public OrdersController(FoodFastDbContext database, 
        IHubContext<OrderHub> orderHub,
        IHubContext<RestaurantHub> restaurantHub)
    {
        _database = database;
        _orderHub = orderHub;
        _restaurantHub = restaurantHub;
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] CreateOrderDto dto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        var order = new Domain.Entities.Order
        {
            CustomerId = userId,
            RestaurantId = dto.RestaurantId,
            Status = "Confirmed",
            CreatedAt = DateTime.UtcNow
        };

        _database.Orders.Add(order);
        await _database.SaveChangesAsync();

        var payload = new
        {
            orderId = order.Id,
            restaurantId = order.RestaurantId,
            status = order.Status,
            customerId = userId
        };

        // Notify Users (for Order Status) [SSE]
        _ = _orderHub.Clients.Group($"order_{order.Id}")
            .SendAsync("NewOrder", payload);

        // Notify Restaurant staff (for Order Creation) [SSE]
        _ = _restaurantHub.Clients.Group($"restaurant_{dto.RestaurantId}")
            .SendAsync("NewOrder", payload);

        return CreatedAtAction(nameof(Get), new { id = order.Id }, order);
    }

    [HttpPut("{id}/status")]
    [Authorize]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusDto dto)
    {
        var order = await _database.Orders.FindAsync(id);
        if (order is null)
        {
            return NotFound();
        }

        order.Status = dto.Status;
        await _database.SaveChangesAsync();

        var payload = new
        {
            orderId = order.Id,
            restaurantId = order.RestaurantId,
            status = order.Status
        };

        // _ = async_task() ---- means that the task is fired and forgot at the same time, don't wait for result which reduce latency

        // Notify Users
        _ = _orderHub.Clients.Group($"order_{order.Id}")
            .SendAsync("OrderUpdated", payload);

        // Notify Restaurant staff
        _ = _restaurantHub.Clients.Group($"restaurant_{order.RestaurantId}")
            .SendAsync("OrderUpdated", payload);

        return Ok(payload);
    }

    [HttpGet("{id}")]
    public IActionResult Get(int id)
    {
        var order = _database.Orders.Find(id);
        if (order is null)
        {
            return NotFound();
        }

        return Ok(order);
    }

    public record CreateOrderDto(int RestaurantId);
    public record UpdateStatusDto(string Status);
}
