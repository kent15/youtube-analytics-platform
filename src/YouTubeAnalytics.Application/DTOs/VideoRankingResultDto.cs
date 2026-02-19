namespace YouTubeAnalytics.Application.DTOs;

public class VideoRankingResultDto
{
    public int TotalCount { get; set; }
    public long TotalViewCount { get; set; }
    public long AverageViewCount { get; set; }
    public double AverageLikeRate { get; set; }
    public List<VideoRankingItemDto> Items { get; set; } = new();
}

public class VideoRankingItemDto
{
    public int Rank { get; set; }
    public string VideoId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string ChannelId { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
    public DateTime PublishedAt { get; set; }
    public long ViewCount { get; set; }
    public long LikeCount { get; set; }
    public long CommentCount { get; set; }
    public double LikeRate { get; set; }
}
