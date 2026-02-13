namespace YouTubeAnalytics.Application.DTOs;

public class AnalysisResultDto
{
    public ChannelInfoDto Channel { get; set; } = new();
    public List<VideoDetailDto> RecentVideos { get; set; } = new();
    public int RecentVideoCount { get; set; }
    public double AverageViewCount { get; set; }
    public string GrowthTrend { get; set; } = string.Empty;
    public string PublishingFrequency { get; set; } = string.Empty;
    public string ContentStrategy { get; set; } = string.Empty;
    public List<ChannelSnapshotDto> Snapshots { get; set; } = new();
}
