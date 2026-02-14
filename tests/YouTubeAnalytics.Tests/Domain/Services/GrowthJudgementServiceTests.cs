using Xunit;
using YouTubeAnalytics.Domain.Entities;
using YouTubeAnalytics.Domain.Enums;
using YouTubeAnalytics.Domain.Services;

namespace YouTubeAnalytics.Tests.Domain.Services;

public class GrowthJudgementServiceTests
{
    private readonly GrowthJudgementService _service = new(1.5);

    [Fact]
    public void Judge_WithNoRecentVideos_ReturnsDeclining()
    {
        var channel = new Channel("UC123", "Test", 1000, 50000, 100, "UU123", DateTime.UtcNow);

        var result = _service.Judge(channel, Array.Empty<Video>());

        Assert.Equal(GrowthTrend.Declining, result);
    }

    [Fact]
    public void Judge_WithZeroVideoCount_ReturnsStable()
    {
        var channel = new Channel("UC123", "Test", 1000, 0, 0, "UU123", DateTime.UtcNow);
        var videos = new List<Video>
        {
            new("V1", "UC123", "Video1", DateTime.UtcNow, 100, 10, 1)
        };

        var result = _service.Judge(channel, videos);

        Assert.Equal(GrowthTrend.Stable, result);
    }

    [Fact]
    public void Judge_WithHighRecentViews_ReturnsGrowing()
    {
        // Channel average: 50000 / 100 = 500 views/video
        // Recent average: 1000 views (>= 500 * 1.5 = 750)
        var channel = new Channel("UC123", "Test", 1000, 50000, 100, "UU123", DateTime.UtcNow);
        var videos = new List<Video>
        {
            new("V1", "UC123", "Video1", DateTime.UtcNow, 1000, 10, 1),
            new("V2", "UC123", "Video2", DateTime.UtcNow, 1000, 10, 1)
        };

        var result = _service.Judge(channel, videos);

        Assert.Equal(GrowthTrend.Growing, result);
    }

    [Fact]
    public void Judge_WithLowRecentViews_ReturnsDeclining()
    {
        // Channel average: 50000 / 100 = 500 views/video
        // Recent average: 100 views (< 500 / 1.5 = 333)
        var channel = new Channel("UC123", "Test", 1000, 50000, 100, "UU123", DateTime.UtcNow);
        var videos = new List<Video>
        {
            new("V1", "UC123", "Video1", DateTime.UtcNow, 100, 10, 1),
            new("V2", "UC123", "Video2", DateTime.UtcNow, 100, 10, 1)
        };

        var result = _service.Judge(channel, videos);

        Assert.Equal(GrowthTrend.Declining, result);
    }

    [Fact]
    public void Judge_WithModerateRecentViews_ReturnsStable()
    {
        // Channel average: 50000 / 100 = 500 views/video
        // Recent average: 500 views (between 333 and 750)
        var channel = new Channel("UC123", "Test", 1000, 50000, 100, "UU123", DateTime.UtcNow);
        var videos = new List<Video>
        {
            new("V1", "UC123", "Video1", DateTime.UtcNow, 500, 10, 1),
            new("V2", "UC123", "Video2", DateTime.UtcNow, 500, 10, 1)
        };

        var result = _service.Judge(channel, videos);

        Assert.Equal(GrowthTrend.Stable, result);
    }
}
