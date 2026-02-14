using Xunit;
using YouTubeAnalytics.Domain.Entities;
using YouTubeAnalytics.Domain.Enums;
using YouTubeAnalytics.Domain.Services;

namespace YouTubeAnalytics.Tests.Domain.Services;

public class PublishingPatternServiceTests
{
    // highFrequencyPerWeek=5, mediumFrequencyPerWeek=2, topPercent=20, shareThreshold=80
    private readonly PublishingPatternService _service = new(5, 2, 20, 80);

    [Fact]
    public void JudgeFrequency_WithNoVideos_ReturnsLow()
    {
        var result = _service.JudgeFrequency(Array.Empty<Video>());

        Assert.Equal(PublishingFrequency.Low, result);
    }

    [Fact]
    public void JudgeFrequency_WithHighFrequency_ReturnsHigh()
    {
        var now = DateTime.UtcNow;
        var videos = Enumerable.Range(0, 10)
            .Select(i => new Video($"V{i}", "UC123", $"Video{i}", now.AddDays(-i), 100, 10, 1))
            .ToList();

        var result = _service.JudgeFrequency(videos);

        Assert.Equal(PublishingFrequency.High, result);
    }

    [Fact]
    public void JudgeFrequency_WithLowFrequency_ReturnsLow()
    {
        var now = DateTime.UtcNow;
        var videos = new List<Video>
        {
            new("V1", "UC123", "Video1", now, 100, 10, 1),
            new("V2", "UC123", "Video2", now.AddDays(-30), 100, 10, 1)
        };

        var result = _service.JudgeFrequency(videos);

        Assert.Equal(PublishingFrequency.Low, result);
    }

    [Fact]
    public void JudgeContentStrategy_WithNoVideos_ReturnsStable()
    {
        var result = _service.JudgeContentStrategy(Array.Empty<Video>());

        Assert.Equal(ContentStrategy.Stable, result);
    }

    [Fact]
    public void JudgeContentStrategy_WithEvenViews_ReturnsStable()
    {
        var now = DateTime.UtcNow;
        var videos = Enumerable.Range(0, 10)
            .Select(i => new Video($"V{i}", "UC123", $"Video{i}", now.AddDays(-i), 100, 10, 1))
            .ToList();

        var result = _service.JudgeContentStrategy(videos);

        Assert.Equal(ContentStrategy.Stable, result);
    }

    [Fact]
    public void JudgeContentStrategy_WithViralVideo_ReturnsViralDependent()
    {
        var now = DateTime.UtcNow;
        var videos = new List<Video>
        {
            new("V1", "UC123", "Viral", now, 100000, 10, 1),
            new("V2", "UC123", "Normal1", now.AddDays(-1), 100, 10, 1),
            new("V3", "UC123", "Normal2", now.AddDays(-2), 100, 10, 1),
            new("V4", "UC123", "Normal3", now.AddDays(-3), 100, 10, 1),
            new("V5", "UC123", "Normal4", now.AddDays(-4), 100, 10, 1)
        };

        var result = _service.JudgeContentStrategy(videos);

        Assert.Equal(ContentStrategy.ViralDependent, result);
    }
}
