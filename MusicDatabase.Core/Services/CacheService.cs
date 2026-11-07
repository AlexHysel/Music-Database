using System.Text.Json;
using StackExchange.Redis;

namespace MusicDatabase.Core.Services;

public class CacheService
{
    private readonly IDatabase _db;

    public CacheService(IConnectionMultiplexer redis)
    {
        _db = redis.GetDatabase();
    }

    public async Task<T?> GetCachedResult<T>(string url)
    {
        RedisValue value = await _db.StringGetAsync(url);

        if (value.IsNullOrEmpty) return default;

        return JsonSerializer.Deserialize<T>((string) value!);
    }

    public async Task SetCachedResult<T>(string url, T result)
    {
        string json = JsonSerializer.Serialize<T>(result);

        if (json.Length > 0)
        {
            await _db.StringSetAsync(url, json, TimeSpan.FromMinutes(5));
        }
    }
}