namespace FoodFast.API.Domain.Models;

public class TokenSettings
{
    public required string Key { get; set; }
    public required string Issuer { get; set; }
    public required string Audience { get; set; }
    public required string TokenExpirationInMinutes { get; set; }
}
