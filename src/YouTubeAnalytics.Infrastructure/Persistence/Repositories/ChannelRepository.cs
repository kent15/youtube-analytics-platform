using Dapper;
using Npgsql;
using YouTubeAnalytics.Domain.Entities;
using YouTubeAnalytics.Domain.Repositories;

namespace YouTubeAnalytics.Infrastructure.Persistence.Repositories;

public class ChannelRepository : IChannelRepository
{
    private readonly string _connectionString;

    public ChannelRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<Channel?> GetByIdAsync(string channelId, CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        var row = await connection.QuerySingleOrDefaultAsync<ChannelRow>(
            @"SELECT channel_id, channel_name, subscriber_count, total_view_count,
                     video_count, uploads_playlist_id, retrieved_at
              FROM channels WHERE channel_id = @ChannelId",
            new { ChannelId = channelId });

        if (row == null)
            return null;

        return new Channel(
            row.channel_id,
            row.channel_name,
            row.subscriber_count,
            row.total_view_count,
            row.video_count,
            row.uploads_playlist_id,
            row.retrieved_at);
    }

    public async Task SaveAsync(Channel channel, CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.ExecuteAsync(
            @"INSERT INTO channels (channel_id, channel_name, subscriber_count, total_view_count,
                                    video_count, uploads_playlist_id, retrieved_at)
              VALUES (@ChannelId, @ChannelName, @SubscriberCount, @TotalViewCount,
                      @VideoCount, @UploadsPlaylistId, @RetrievedAt)
              ON CONFLICT (channel_id) DO UPDATE SET
                  channel_name = EXCLUDED.channel_name,
                  subscriber_count = EXCLUDED.subscriber_count,
                  total_view_count = EXCLUDED.total_view_count,
                  video_count = EXCLUDED.video_count,
                  uploads_playlist_id = EXCLUDED.uploads_playlist_id,
                  retrieved_at = EXCLUDED.retrieved_at",
            new
            {
                channel.ChannelId,
                channel.ChannelName,
                channel.SubscriberCount,
                channel.TotalViewCount,
                channel.VideoCount,
                channel.UploadsPlaylistId,
                channel.RetrievedAt
            });
    }

    private class ChannelRow
    {
        public string channel_id { get; set; } = string.Empty;
        public string channel_name { get; set; } = string.Empty;
        public long subscriber_count { get; set; }
        public long total_view_count { get; set; }
        public long video_count { get; set; }
        public string uploads_playlist_id { get; set; } = string.Empty;
        public DateTime retrieved_at { get; set; }
    }
}
