using System.ComponentModel.DataAnnotations;

namespace FoodFast.API.Domain.Entities;

public class ChatMessage
{
    [Key]
    public int Id { get; set; }
    public string ConversationId { get; set; } = default!;
    public string SenderId { get; set; } = default!;
    public string Text { get; set; } = default!;
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    public bool Delivered { get; set; } = false;
    public bool Read { get; set; } = false;
}
