using YouTubeAnalytics.Application.Interfaces;

namespace YouTubeAnalytics.Tests.Integration.Stubs;

public class StubCacheService : ICacheService
{
    private readonly Dictionary<string, object> _cache = new();

    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        if (_cache.TryGetValue(key, out var value))
            return Task.FromResult(value as T);

        return Task.FromResult<T?>(null);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan expiry, CancellationToken cancellationToken = default) where T : class
    {
        _cache[key] = value;
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        _cache.Remove(key);
        return Task.CompletedTask;
    }
}
