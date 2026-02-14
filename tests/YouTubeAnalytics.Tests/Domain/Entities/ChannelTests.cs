using Xunit;
using YouTubeAnalytics.Domain.Entities;

namespace YouTubeAnalytics.Tests.Domain.Entities;

public class ChannelTests
{
    [Fact]
    public void Constructor_WithValidParameters_CreatesInstance()
    {
        var channel = new Channel("UC123", "TestChannel", 1000, 50000, 100, "UU123", DateTime.UtcNow);

        Assert.Equal("UC123", channel.ChannelId);
        Assert.Equal("TestChannel", channel.ChannelName);
        Assert.Equal(1000, channel.SubscriberCount);
        Assert.Equal(50000, channel.TotalViewCount);
        Assert.Equal(100, channel.VideoCount);
        Assert.Equal("UU123", channel.UploadsPlaylistId);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidChannelId_ThrowsArgumentException(string? channelId)
    {
        Assert.Throws<ArgumentException>(() =>
            new Channel(channelId!, "Name", 0, 0, 0, "UU123", DateTime.UtcNow));
    }

    [Fact]
    public void Constructor_WithNegativeSubscriberCount_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Channel("UC123", "Name", -1, 0, 0, "UU123", DateTime.UtcNow));
    }

    [Fact]
    public void Constructor_WithNegativeViewCount_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Channel("UC123", "Name", 0, -1, 0, "UU123", DateTime.UtcNow));
    }

    [Fact]
    public void Constructor_WithNegativeVideoCount_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Channel("UC123", "Name", 0, 0, -1, "UU123", DateTime.UtcNow));
    }
}
