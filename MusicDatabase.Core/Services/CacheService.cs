using System.Text.Json;
using StackExchange.Redis;

namespace MusicDatabase.Core.Services;

public class CacheService
{
    private readonly IConnectionMultiplexer? _redis;
    private readonly IDatabase? _db;

    public CacheService(IConnectionMultiplexer redis)
    {
        _redis = redis;
        _db = redis.GetDatabase();
    }

    public static ConfigurationOptions CreateConfiguration(string? connectionString)
    {
        var options = ConfigurationOptions.Parse(string.IsNullOrWhiteSpace(connectionString)
            ? "localhost:6379"
            : connectionString);

        options.AbortOnConnectFail = false;
        options.ConnectRetry = 5;
        options.ConnectTimeout = 5000;
        options.SyncTimeout = 5000;

        return options;
    }

    public async Task<T?> GetCachedResult<T>(string url)
    {
        if (_redis is null || !_redis.IsConnected || _db is null)
            return default;

        RedisValue value = await _db.StringGetAsync(url);

        if (value.IsNullOrEmpty) return default;

        return JsonSerializer.Deserialize<T>((string)value!);
    }

    public async Task SetCachedResult<T>(string url, T result)
    {
        if (_redis is null || !_redis.IsConnected || _db is null)
            return;

        string json = JsonSerializer.Serialize(result);

        if (json.Length > 0)
        {
            await _db.StringSetAsync(url, json, TimeSpan.FromMinutes(5));
        }
    }

    public async Task DeleteKey(string url)
    {
        if (_redis is null || !_redis.IsConnected || _db is null)
            return;

        await _db.KeyDeleteAsync(url);
    }
}