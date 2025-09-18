using FoodFast.API.Data;
using FoodFast.API.Domain.Entities;
using Microsoft.AspNetCore.SignalR;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace FoodFast.API.Hubs;

public class ChatHub : Hub
{
    private readonly FoodFastDbContext _context;
    public ChatHub(FoodFastDbContext context) => _context = context;

    public async Task JoinConversation(string convId) => 
        await Groups.AddToGroupAsync(Context.ConnectionId, $"conv_{convId}");

    public async Task LeaveConversation(string convId) =>
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"conv_{convId}");

    public async Task SendMessage(string convId, string text)
    {
        var senderId = Context.UserIdentifier ?? Context.User?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        var message = new ChatMessage { ConversationId = convId, SenderId = senderId!, Text = text, SentAt = DateTime.UtcNow };
        _context.ChatMessages.Add(message);
        await _context.SaveChangesAsync();

        await Clients.Group($"conv_{convId}").SendAsync("ReceiveMessage", 
            new { 
                id = message.Id, 
                sender = message.SenderId, 
                text = message.Text, 
                ts = message.SentAt 
            });
    }

    public async Task Typing(string convId)
    {
        await Clients.OthersInGroup($"conv_{convId}").SendAsync("Typing", Context.UserIdentifier);
    }

    public async Task AcknowledgeMessage(int messageId)
    {
        var message = await _context.ChatMessages.FindAsync(messageId);
        if (message is not null)
        {
            message.Delivered = true;
            await _context.SaveChangesAsync();
        }
    }
}