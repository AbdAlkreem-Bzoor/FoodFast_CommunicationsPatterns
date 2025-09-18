namespace FoodFast.API.Services;

public interface ITokenBlacklistService
{
    Task BlacklistAsync(string jti, TimeSpan expiresIn);
    Task<bool> IsBlacklistedAsync(string jti);
}