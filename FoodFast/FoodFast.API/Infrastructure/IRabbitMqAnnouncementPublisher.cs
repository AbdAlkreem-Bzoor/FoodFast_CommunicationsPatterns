using FoodFast.API.Domain.Entities;

namespace FoodFast.API.Infrastructure;

public interface IRabbitMqAnnouncementPublisher
{
    Task PublishAsync(Announcement announcement);
}
