using Xunit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using YouTubeAnalytics.Infrastructure.YouTube;

namespace YouTubeAnalytics.Tests.Infrastructure.YouTube;

public class QuotaManagerTests
{
    private readonly ILogger<QuotaManager> _logger = NullLogger<QuotaManager>.Instance;

    [Fact]
    public async Task CheckAndReserveAsync_WithinLimit_Succeeds()
    {
        var manager = new QuotaManager(100, 80, _logger);

        await manager.CheckAndReserveAsync(10);

        var remaining = await manager.GetRemainingQuotaAsync();
        Assert.Equal(90, remaining);
    }

    [Fact]
    public async Task CheckAndReserveAsync_ExceedingLimit_ThrowsInvalidOperationException()
    {
        var manager = new QuotaManager(10, 8, _logger);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => manager.CheckAndReserveAsync(11));
    }

    [Fact]
    public async Task CheckAndReserveAsync_AccumulatedExceedsLimit_ThrowsInvalidOperationException()
    {
        var manager = new QuotaManager(10, 8, _logger);

        await manager.CheckAndReserveAsync(6);
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => manager.CheckAndReserveAsync(5));
    }

    [Fact]
    public async Task GetRemainingQuotaAsync_InitialState_ReturnsFullQuota()
    {
        var manager = new QuotaManager(10000, 8000, _logger);

        var remaining = await manager.GetRemainingQuotaAsync();

        Assert.Equal(10000, remaining);
    }

    [Fact]
    public async Task CheckAndReserveAsync_ExactLimit_Succeeds()
    {
        var manager = new QuotaManager(10, 8, _logger);

        await manager.CheckAndReserveAsync(10);

        var remaining = await manager.GetRemainingQuotaAsync();
        Assert.Equal(0, remaining);
    }
}
