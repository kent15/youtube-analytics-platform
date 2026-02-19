using Dapper;
using Npgsql;
using YouTubeAnalytics.Domain.Entities;
using YouTubeAnalytics.Domain.Repositories;

namespace YouTubeAnalytics.Infrastructure.Persistence.Repositories;

public class VideoRepository : IVideoRepository
{
    private readonly string _connectionString;

    public VideoRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<IReadOnlyList<Video>> GetByChannelIdAsync(string channelId, CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        var rows = await connection.QueryAsync<VideoRow>(
            @"SELECT video_id, channel_id, title, published_at, view_count, like_count, comment_count
              FROM videos WHERE channel_id = @ChannelId
              ORDER BY published_at DESC",
            new { ChannelId = channelId });

        return rows.Select(r => new Video(
            r.video_id,
            r.channel_id,
            r.title,
            r.published_at,
            r.view_count,
            r.like_count,
            r.comment_count)).ToList();
    }

    public async Task<IReadOnlyList<Video>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        var rows = await connection.QueryAsync<VideoRow>(
            @"SELECT video_id, channel_id, title, published_at, view_count, like_count, comment_count
              FROM videos
              ORDER BY published_at DESC");

        return rows.Select(r => new Video(
            r.video_id,
            r.channel_id,
            r.title,
            r.published_at,
            r.view_count,
            r.like_count,
            r.comment_count)).ToList();
    }

    public async Task SaveManyAsync(IEnumerable<Video> videos, CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        foreach (var video in videos)
        {
            await connection.ExecuteAsync(
                @"INSERT INTO videos (video_id, channel_id, title, published_at, view_count, like_count, comment_count)
                  VALUES (@VideoId, @ChannelId, @Title, @PublishedAt, @ViewCount, @LikeCount, @CommentCount)
                  ON CONFLICT (video_id) DO UPDATE SET
                      title = EXCLUDED.title,
                      view_count = EXCLUDED.view_count,
                      like_count = EXCLUDED.like_count,
                      comment_count = EXCLUDED.comment_count",
                new
                {
                    video.VideoId,
                    video.ChannelId,
                    video.Title,
                    video.PublishedAt,
                    video.ViewCount,
                    video.LikeCount,
                    video.CommentCount
                });
        }
    }

    private class VideoRow
    {
        public string video_id { get; set; } = string.Empty;
        public string channel_id { get; set; } = string.Empty;
        public string title { get; set; } = string.Empty;
        public DateTime published_at { get; set; }
        public long view_count { get; set; }
        public long like_count { get; set; }
        public long comment_count { get; set; }
    }
}
