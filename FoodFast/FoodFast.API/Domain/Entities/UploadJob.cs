using System.ComponentModel.DataAnnotations;

namespace FoodFast.API.Domain.Entities;

public class UploadJob
{
    [Key]
    public Guid Id { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public string? ResultPath { get; set; }
    public string Status { get; set; } = "Pending"; // Pending, Processing, Completed, Failed
    public string? OwnerUserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public string? Error { get; set; }
}
