namespace FoodFast.API.Domain.Models;

public class EmailSettings
{
    public required string Server { get; set; }
    public required int Port { get; set; }
    public required string UserName { get; set; }
    public required string Password { get; set; }
    public required string FromEmail { get; set; }
}