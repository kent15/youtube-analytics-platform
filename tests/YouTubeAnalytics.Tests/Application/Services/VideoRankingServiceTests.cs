using YouTubeAnalytics.Application.Services;
using YouTubeAnalytics.Domain.Entities;
using Xunit;

namespace YouTubeAnalytics.Tests.Application.Services;

public class VideoRankingServiceTests
{
    [Fact]
    public void FilterByPeriod_ZeroDays_ReturnsAll()
    {
        var videos = CreateSampleVideos();
        var result = VideoRankingService.FilterByPeriod(videos, 0);
        Assert.Equal(videos.Count, result.Count);
    }

    [Fact]
    public void FilterByPeriod_7Days_ExcludesOldVideos()
    {
        var videos = CreateSampleVideos();
        var result = VideoRankingService.FilterByPeriod(videos, 7);
        Assert.All(result, v => Assert.True(v.PublishedAt >= DateTime.UtcNow.AddDays(-7)));
    }

    [Fact]
    public void SortVideos_ByViewCount_DescendingOrder()
    {
        var videos = CreateSampleVideos();
        var sorted = VideoRankingService.SortVideos(videos, "viewCount").ToList();
        for (int i = 1; i < sorted.Count; i++)
            Assert.True(sorted[i - 1].ViewCount >= sorted[i].ViewCount);
    }

    [Fact]
    public void SortVideos_ByLikeCount_DescendingOrder()
    {
        var videos = CreateSampleVideos();
        var sorted = VideoRankingService.SortVideos(videos, "likeCount").ToList();
        for (int i = 1; i < sorted.Count; i++)
            Assert.True(sorted[i - 1].LikeCount >= sorted[i].LikeCount);
    }

    [Fact]
    public void SortVideos_ByCommentCount_DescendingOrder()
    {
        var videos = CreateSampleVideos();
        var sorted = VideoRankingService.SortVideos(videos, "commentCount").ToList();
        for (int i = 1; i < sorted.Count; i++)
            Assert.True(sorted[i - 1].CommentCount >= sorted[i].CommentCount);
    }

    [Fact]
    public void SortVideos_ByLikeRate_DescendingOrder()
    {
        var videos = CreateSampleVideos();
        var sorted = VideoRankingService.SortVideos(videos, "likeRate").ToList();
        for (int i = 1; i < sorted.Count; i++)
        {
            var prevRate = sorted[i - 1].ViewCount > 0 ? (double)sorted[i - 1].LikeCount / sorted[i - 1].ViewCount : 0;
            var curRate = sorted[i].ViewCount > 0 ? (double)sorted[i].LikeCount / sorted[i].ViewCount : 0;
            Assert.True(prevRate >= curRate);
        }
    }

    [Fact]
    public void SortVideos_InvalidSortBy_FallsBackToViewCount()
    {
        var videos = CreateSampleVideos();
        var sorted = VideoRankingService.SortVideos(videos, "invalid").ToList();
        for (int i = 1; i < sorted.Count; i++)
            Assert.True(sorted[i - 1].ViewCount >= sorted[i].ViewCount);
    }

    [Fact]
    public void FilterByPeriod_EmptyList_ReturnsEmpty()
    {
        var result = VideoRankingService.FilterByPeriod(Array.Empty<Video>(), 30);
        Assert.Empty(result);
    }

    private static List<Video> CreateSampleVideos()
    {
        return new List<Video>
        {
            new("V1", "CH1", "High views low likes", DateTime.UtcNow.AddDays(-1), 100000, 500, 50),
            new("V2", "CH1", "Medium views high likes", DateTime.UtcNow.AddDays(-3), 50000, 5000, 200),
            new("V3", "CH2", "Low views lots of comments", DateTime.UtcNow.AddDays(-5), 10000, 300, 800),
            new("V4", "CH2", "Old video", DateTime.UtcNow.AddDays(-60), 200000, 10000, 1500),
        };
    }
}
