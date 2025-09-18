namespace FoodFast.API.Domain.Models;

public class RabbitMqSetting
{
    public required string HostName { get; set; }
    public required string UserName { get; set; }
    public required string Password { get; set; }
    public required string ConnectionString { get; set; }
    public required string ExchangeName { get; set; }
    public required string QueueName { get; set; }
    public required string RoutingKey { get; set; }
    public required string Expiration { get; set; }
}
