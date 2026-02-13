namespace YouTubeAnalytics.Application.DTOs;

public class ChannelSnapshotDto
{
    public DateTime RecordedAt { get; set; }
    public long SubscriberCount { get; set; }
    public long TotalViewCount { get; set; }
}
