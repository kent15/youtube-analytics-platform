using Microsoft.Extensions.Logging;

namespace YouTubeAnalytics.Infrastructure.YouTube;

public class QuotaManager
{
    private readonly int _dailyQuotaLimit;
    private readonly int _alertThreshold;
    private readonly ILogger<QuotaManager> _logger;
    private int _usedQuota;
    private DateTime _resetDate;
    private readonly object _lock = new();

    public QuotaManager(int dailyQuotaLimit, int alertThreshold, ILogger<QuotaManager> logger)
    {
        _dailyQuotaLimit = dailyQuotaLimit;
        _alertThreshold = alertThreshold;
        _logger = logger;
        _usedQuota = 0;
        _resetDate = GetNextResetTime();
    }

    public Task CheckAndReserveAsync(int requiredUnits, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            ResetIfNeeded();

            if (_usedQuota + requiredUnits > _dailyQuotaLimit)
            {
                _logger.LogWarning("Quota exceeded. Used: {Used}, Requested: {Requested}, Limit: {Limit}",
                    _usedQuota, requiredUnits, _dailyQuotaLimit);
                throw new InvalidOperationException(
                    $"YouTube API quota exceeded. Used: {_usedQuota}/{_dailyQuotaLimit}. Reset at PST midnight.");
            }

            _usedQuota += requiredUnits;

            if (_usedQuota >= _alertThreshold)
            {
                _logger.LogWarning("Quota alert threshold reached: {Used}/{Limit}", _usedQuota, _dailyQuotaLimit);
            }
        }

        return Task.CompletedTask;
    }

    public Task<int> GetRemainingQuotaAsync(CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            ResetIfNeeded();
            return Task.FromResult(_dailyQuotaLimit - _usedQuota);
        }
    }

    private void ResetIfNeeded()
    {
        if (DateTime.UtcNow >= _resetDate)
        {
            _logger.LogInformation("Resetting daily quota counter");
            _usedQuota = 0;
            _resetDate = GetNextResetTime();
        }
    }

    private static DateTime GetNextResetTime()
    {
        var pst = TimeZoneInfo.FindSystemTimeZoneById("America/Los_Angeles");
        var nowPst = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, pst);
        var nextMidnightPst = nowPst.Date.AddDays(1);
        return TimeZoneInfo.ConvertTimeToUtc(nextMidnightPst, pst);
    }
}
