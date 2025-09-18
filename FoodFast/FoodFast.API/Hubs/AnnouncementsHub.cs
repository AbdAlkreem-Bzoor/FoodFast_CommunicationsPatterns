using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace FoodFast.API.Hubs;

public class AnnouncementsHub : Hub
{
    private static readonly ConcurrentDictionary<string, HashSet<string>> _usersConnections = new();

    public override Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier!;
        _usersConnections.TryAdd(userId, new HashSet<string>());
        _usersConnections[userId].Add(Context.ConnectionId);
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.UserIdentifier!;
        if (_usersConnections.TryGetValue(userId, out var connections))
        {
            connections.Remove(Context.ConnectionId);
            if (connections.Count == 0)
            {
                _usersConnections.TryRemove(userId, out _);
            }
        }
        return base.OnDisconnectedAsync(exception);
    }

    public static bool IsUserOnline(string userId)
    {
        return _usersConnections.ContainsKey(userId);
    }

    public async Task JoinAnnouncementGroup(int announcementId) =>
         await Groups.AddToGroupAsync(Context.ConnectionId, $"announcement_{announcementId}");

    public async Task LeaveAnnouncementGroup(int announcementId) =>
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"announcement_{announcementId}");
}
