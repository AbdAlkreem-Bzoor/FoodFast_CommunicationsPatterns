using FoodFast.API.Data;
using FoodFast.API.Domain.Entities;
using FoodFast.API.Hubs;
using FoodFast.API.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace FoodFast.API.Controllers;

[ApiController]
[Route("api/announcements")]
public class AnnouncementsController : ControllerBase
{
    private readonly FoodFastDbContext _database;
    private readonly IHubContext<AnnouncementsHub> _hub;
    private readonly IRabbitMqAnnouncementPublisher _publisher;

    public AnnouncementsController(FoodFastDbContext database,
                                   IHubContext<AnnouncementsHub> hub,
                                   IRabbitMqAnnouncementPublisher publisher)
    {
        _database = database;
        _hub = hub;
        _publisher = publisher;
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] AnnouncementDto dto)
    {
        var announcement = new Announcement
        {
            Title = dto.Title,
            Body = dto.Body,
            PublishedAt = DateTime.UtcNow
        };

        _database.Announcements.Add(announcement);

        await _database.SaveChangesAsync();

        _ = _publisher.PublishAsync(announcement);

        return Ok(announcement);
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> List(int page = 1, int pageSize = 50)
    {
        var items = await
            _database.Announcements
            .OrderByDescending(a => a.PublishedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(items);
    }

    public record AnnouncementDto(string Title, string Body);
}