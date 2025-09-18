using FoodFast.API.Domain.Entities;
using Microsoft.AspNetCore.SignalR;

namespace FoodFast.API.Hubs;

public class RestaurantHub : Hub
{
    public async Task JoinRestaurant(int restaurantId) => 
        await Groups.AddToGroupAsync(Context.ConnectionId, $"restaurant_{restaurantId}");
    public async Task LeaveRestaurant(int restaurantId) => 
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"restaurant_{restaurantId}");

    public async Task NotifyNewOrder(int restaurantId, OrderDto order)
    {
        await Clients.Group($"restaurant_{restaurantId}").SendAsync("ReceiveOrder", order);
    }
}

public record OrderDto(string CustomerId, int RestaurantId, string Status, DateTime CreatedAt);