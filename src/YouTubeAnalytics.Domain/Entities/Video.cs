namespace YouTubeAnalytics.Domain.Entities;

public class Video
{
    public string VideoId { get; }
    public string ChannelId { get; }
    public string Title { get; }
    public DateTime PublishedAt { get; }
    public long ViewCount { get; }
    public long LikeCount { get; }
    public long CommentCount { get; }

    public Video(
        string videoId,
        string channelId,
        string title,
        DateTime publishedAt,
        long viewCount,
        long likeCount,
        long commentCount)
    {
        if (string.IsNullOrWhiteSpace(videoId))
            throw new ArgumentException("Video ID is required.", nameof(videoId));
        if (viewCount < 0)
            throw new ArgumentOutOfRangeException(nameof(viewCount));
        if (likeCount < 0)
            throw new ArgumentOutOfRangeException(nameof(likeCount));
        if (commentCount < 0)
            throw new ArgumentOutOfRangeException(nameof(commentCount));

        VideoId = videoId;
        ChannelId = channelId;
        Title = title;
        PublishedAt = publishedAt;
        ViewCount = viewCount;
        LikeCount = likeCount;
        CommentCount = commentCount;
    }
}
