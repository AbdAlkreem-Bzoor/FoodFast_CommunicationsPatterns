using StackExchange.Redis;


namespace FoodFast.API.Services;

public class RedisTokenBlacklistService : ITokenBlacklistService
{
    private readonly IConnectionMultiplexer _redis;
    public RedisTokenBlacklistService(IConnectionMultiplexer redis) 
    { 
        _redis = redis; 
    }

    private static string Key(string jti) => $"revoked_jti:{jti}";

    public async Task BlacklistAsync(string jti, TimeSpan expiresIn)
    {
        var database = _redis.GetDatabase();
        await database.StringSetAsync(Key(jti), "1", expiresIn);
    }

    public async Task<bool> IsBlacklistedAsync(string jti)
    {
        var database = _redis.GetDatabase();
        return await database.KeyExistsAsync(Key(jti));
    }
}