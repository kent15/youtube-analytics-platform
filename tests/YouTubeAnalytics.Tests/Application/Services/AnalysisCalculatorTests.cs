using Xunit;
using YouTubeAnalytics.Application.Services;
using YouTubeAnalytics.Domain.Entities;

namespace YouTubeAnalytics.Tests.Application.Services;

public class AnalysisCalculatorTests
{
    [Fact]
    public void CountRecentVideos_WithRecentVideos_ReturnsCorrectCount()
    {
        var now = DateTime.UtcNow;
        var videos = new List<Video>
        {
            new("V1", "UC123", "Recent1", now.AddDays(-5), 100, 10, 1),
            new("V2", "UC123", "Recent2", now.AddDays(-10), 100, 10, 1),
            new("V3", "UC123", "Old", now.AddDays(-60), 100, 10, 1)
        };

        var count = AnalysisCalculator.CountRecentVideos(videos, 30);

        Assert.Equal(2, count);
    }

    [Fact]
    public void CountRecentVideos_WithNoVideos_ReturnsZero()
    {
        var count = AnalysisCalculator.CountRecentVideos(Array.Empty<Video>(), 30);

        Assert.Equal(0, count);
    }

    [Fact]
    public void CalculateAverageViewCount_WithVideos_ReturnsCorrectAverage()
    {
        var videos = new List<Video>
        {
            new("V1", "UC123", "Video1", DateTime.UtcNow, 100, 10, 1),
            new("V2", "UC123", "Video2", DateTime.UtcNow, 200, 10, 1),
            new("V3", "UC123", "Video3", DateTime.UtcNow, 300, 10, 1)
        };

        var average = AnalysisCalculator.CalculateAverageViewCount(videos);

        Assert.Equal(200.0, average);
    }

    [Fact]
    public void CalculateAverageViewCount_WithNoVideos_ReturnsZero()
    {
        var average = AnalysisCalculator.CalculateAverageViewCount(Array.Empty<Video>());

        Assert.Equal(0, average);
    }
}
