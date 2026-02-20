namespace YouTubeAnalytics.Infrastructure.Configuration;

public class BatchConfig
{
    public bool Enabled { get; set; } = true;
    public string ExecutionTime { get; set; } = "03:00";
    public List<BatchChannelEntry> Channels { get; set; } = new();
}

public class BatchChannelEntry
{
    public string ChannelId { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
}
