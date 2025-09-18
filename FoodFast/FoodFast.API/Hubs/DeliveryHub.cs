using Microsoft.AspNetCore.SignalR;

namespace FoodFast.API.Hubs;

public class DeliveryHub : Hub
{
    public async Task JoinOrderGroup(int orderId) => 
        await Groups.AddToGroupAsync(Context.ConnectionId, $"order_{orderId}");
    public async Task LeaveOrderGroup(int orderId) => 
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"order_{orderId}");

    public async Task SendLocation(int orderId, double lat, double lng, long ts)
    {
        await Clients.Group($"order_{orderId}").SendAsync("ReceiveLocation", new { lat, lng, ts });
    }
}
