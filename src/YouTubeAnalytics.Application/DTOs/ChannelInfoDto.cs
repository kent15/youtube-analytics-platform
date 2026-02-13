namespace YouTubeAnalytics.Application.DTOs;

public class ChannelInfoDto
{
    public string ChannelId { get; set; } = string.Empty;
    public string ChannelName { get; set; } = string.Empty;
    public long SubscriberCount { get; set; }
    public long TotalViewCount { get; set; }
    public long VideoCount { get; set; }
}
