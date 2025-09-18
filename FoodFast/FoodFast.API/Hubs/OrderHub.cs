using Microsoft.AspNetCore.SignalR;

namespace FoodFast.API.Hubs;

public class OrderHub : Hub
{
    public async Task JoinOrderGroup(int orderId) =>
        await Groups.AddToGroupAsync(Context.ConnectionId, $"order_{orderId}");

    public async Task LeaveOrderGroup(int orderId) =>
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"order_{orderId}");
}