using FoodFast.API.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodFast.API.Controllers;

[ApiController]
[Route("api/chat")]
public class ChatController : ControllerBase
{
    private readonly FoodFastDbContext _database;
    public ChatController(FoodFastDbContext database) 
    { 
        _database = database; 
    }

    [HttpGet("conversation/{convId}")]
    [Authorize]
    public async Task<IActionResult> GetConversation(string convId, int page = 1, int pageSize = 50)
    {
        var chat = _database.ChatMessages.Where(c => c.ConversationId == convId)
                                         .OrderByDescending(c => c.SentAt);

        var messages = await chat.Skip((page - 1) * pageSize)
                                 .Take(pageSize)
                                 .ToListAsync();

        return Ok(messages.OrderBy(c => c.SentAt));
    }
}