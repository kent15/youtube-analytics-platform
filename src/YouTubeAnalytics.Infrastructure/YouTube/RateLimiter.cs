namespace YouTubeAnalytics.Infrastructure.YouTube;

public class RateLimiter
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly int _maxRequestsPerSecond;
    private DateTime _lastRequestTime = DateTime.MinValue;

    public RateLimiter(int maxRequestsPerSecond)
    {
        _maxRequestsPerSecond = maxRequestsPerSecond;
    }

    public async Task WaitForPermitAsync(CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            var minInterval = TimeSpan.FromMilliseconds(1000.0 / _maxRequestsPerSecond);
            var elapsed = DateTime.UtcNow - _lastRequestTime;

            if (elapsed < minInterval)
            {
                var delay = minInterval - elapsed;
                await Task.Delay(delay, cancellationToken);
            }

            _lastRequestTime = DateTime.UtcNow;
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
