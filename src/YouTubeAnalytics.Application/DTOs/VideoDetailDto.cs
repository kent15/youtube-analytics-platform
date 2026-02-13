namespace YouTubeAnalytics.Application.DTOs;

public class VideoDetailDto
{
    public string VideoId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
    public DateTime PublishedAt { get; set; }
    public long ViewCount { get; set; }
    public long LikeCount { get; set; }
    public long CommentCount { get; set; }
}
