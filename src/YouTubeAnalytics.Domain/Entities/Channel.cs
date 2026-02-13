namespace YouTubeAnalytics.Domain.Entities;

public class Channel
{
    public string ChannelId { get; }
    public string ChannelName { get; }
    public long SubscriberCount { get; }
    public long TotalViewCount { get; }
    public long VideoCount { get; }
    public string UploadsPlaylistId { get; }
    public DateTime RetrievedAt { get; }

    public Channel(
        string channelId,
        string channelName,
        long subscriberCount,
        long totalViewCount,
        long videoCount,
        string uploadsPlaylistId,
        DateTime retrievedAt)
    {
        if (string.IsNullOrWhiteSpace(channelId))
            throw new ArgumentException("Channel ID is required.", nameof(channelId));
        if (subscriberCount < 0)
            throw new ArgumentOutOfRangeException(nameof(subscriberCount));
        if (totalViewCount < 0)
            throw new ArgumentOutOfRangeException(nameof(totalViewCount));
        if (videoCount < 0)
            throw new ArgumentOutOfRangeException(nameof(videoCount));

        ChannelId = channelId;
        ChannelName = channelName;
        SubscriberCount = subscriberCount;
        TotalViewCount = totalViewCount;
        VideoCount = videoCount;
        UploadsPlaylistId = uploadsPlaylistId;
        RetrievedAt = retrievedAt;
    }
}
