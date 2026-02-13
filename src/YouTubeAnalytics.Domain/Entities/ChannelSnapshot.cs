namespace YouTubeAnalytics.Domain.Entities;

public class ChannelSnapshot
{
    public long Id { get; }
    public string ChannelId { get; }
    public long SubscriberCount { get; }
    public long TotalViewCount { get; }
    public DateTime RecordedAt { get; }

    public ChannelSnapshot(
        long id,
        string channelId,
        long subscriberCount,
        long totalViewCount,
        DateTime recordedAt)
    {
        if (string.IsNullOrWhiteSpace(channelId))
            throw new ArgumentException("Channel ID is required.", nameof(channelId));
        if (subscriberCount < 0)
            throw new ArgumentOutOfRangeException(nameof(subscriberCount));
        if (totalViewCount < 0)
            throw new ArgumentOutOfRangeException(nameof(totalViewCount));

        Id = id;
        ChannelId = channelId;
        SubscriberCount = subscriberCount;
        TotalViewCount = totalViewCount;
        RecordedAt = recordedAt;
    }
}
