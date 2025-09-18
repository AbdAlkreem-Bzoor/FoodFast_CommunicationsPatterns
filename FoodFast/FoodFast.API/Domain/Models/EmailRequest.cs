namespace FoodFast.API.Domain.Models;

public class EmailRequest
{
    public required IEnumerable<string> ToEmails { get; set; }
    public required string Subject { get; set; }
    public required string Message { get; set; }
    public required IEnumerable<Attachment> Attachments { get; set; }
}
