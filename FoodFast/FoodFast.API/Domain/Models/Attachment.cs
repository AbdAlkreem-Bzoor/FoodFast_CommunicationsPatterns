namespace FoodFast.API.Domain.Models;

public class Attachment
{
    public required string Name { get; set; }
    public required string ContentType { get; set; }
    public required byte[] Content { get; set; }
}