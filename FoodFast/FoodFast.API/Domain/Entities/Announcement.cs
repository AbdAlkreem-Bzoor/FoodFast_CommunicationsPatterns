using System.ComponentModel.DataAnnotations;

namespace FoodFast.API.Domain.Entities;

public class Announcement
{
    [Key]
    public int Id { get; set; }
    public string Title { get; set; } = default!;
    public string Body { get; set; } = default!;
    public DateTime PublishedAt { get; set; } = DateTime.UtcNow;
}
