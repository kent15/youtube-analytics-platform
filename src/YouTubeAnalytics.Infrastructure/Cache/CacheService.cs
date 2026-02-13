using System.Text.Json;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using YouTubeAnalytics.Application.Interfaces;

namespace YouTubeAnalytics.Infrastructure.Cache;

public class CacheService : ICacheService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<CacheService> _logger;

    public CacheService(IConnectionMultiplexer redis, ILogger<CacheService> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        var db = _redis.GetDatabase();
        var value = await db.StringGetAsync(key);

        if (value.IsNullOrEmpty)
            return null;

        _logger.LogDebug("Cache hit for key {Key}", key);
        return JsonSerializer.Deserialize<T>(value!);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan expiry, CancellationToken cancellationToken = default) where T : class
    {
        var db = _redis.GetDatabase();
        var json = JsonSerializer.Serialize(value);
        await db.StringSetAsync(key, json, expiry);
        _logger.LogDebug("Cache set for key {Key} with TTL {Expiry}", key, expiry);
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        await db.KeyDeleteAsync(key);
        _logger.LogDebug("Cache removed for key {Key}", key);
    }
}
