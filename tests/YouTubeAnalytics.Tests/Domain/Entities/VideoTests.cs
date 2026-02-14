using Xunit;
using YouTubeAnalytics.Domain.Entities;

namespace YouTubeAnalytics.Tests.Domain.Entities;

public class VideoTests
{
    [Fact]
    public void Constructor_WithValidParameters_CreatesInstance()
    {
        var publishedAt = DateTime.UtcNow;
        var video = new Video("V123", "UC123", "Test Video", publishedAt, 1000, 50, 10);

        Assert.Equal("V123", video.VideoId);
        Assert.Equal("UC123", video.ChannelId);
        Assert.Equal("Test Video", video.Title);
        Assert.Equal(publishedAt, video.PublishedAt);
        Assert.Equal(1000, video.ViewCount);
        Assert.Equal(50, video.LikeCount);
        Assert.Equal(10, video.CommentCount);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidVideoId_ThrowsArgumentException(string? videoId)
    {
        Assert.Throws<ArgumentException>(() =>
            new Video(videoId!, "UC123", "Title", DateTime.UtcNow, 0, 0, 0));
    }

    [Fact]
    public void Constructor_WithNegativeViewCount_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Video("V123", "UC123", "Title", DateTime.UtcNow, -1, 0, 0));
    }

    [Fact]
    public void Constructor_WithNegativeLikeCount_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Video("V123", "UC123", "Title", DateTime.UtcNow, 0, -1, 0));
    }

    [Fact]
    public void Constructor_WithNegativeCommentCount_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Video("V123", "UC123", "Title", DateTime.UtcNow, 0, 0, -1));
    }
}
