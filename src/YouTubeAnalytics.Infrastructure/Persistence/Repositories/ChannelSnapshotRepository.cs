using Dapper;
using Npgsql;
using YouTubeAnalytics.Domain.Entities;
using YouTubeAnalytics.Domain.Repositories;

namespace YouTubeAnalytics.Infrastructure.Persistence.Repositories;

public class ChannelSnapshotRepository : IChannelSnapshotRepository
{
    private readonly string _connectionString;

    public ChannelSnapshotRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<IReadOnlyList<ChannelSnapshot>> GetByChannelIdAsync(string channelId, int days, CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        var cutoff = DateTime.UtcNow.AddDays(-days).Date;

        var rows = await connection.QueryAsync<SnapshotRow>(
            @"SELECT id, channel_id, subscriber_count, total_view_count, recorded_at
              FROM channel_snapshots
              WHERE channel_id = @ChannelId AND recorded_at >= @Cutoff
              ORDER BY recorded_at ASC",
            new { ChannelId = channelId, Cutoff = cutoff });

        return rows.Select(r => new ChannelSnapshot(
            r.id,
            r.channel_id,
            r.subscriber_count,
            r.total_view_count,
            r.recorded_at)).ToList();
    }

    public async Task UpsertAsync(ChannelSnapshot snapshot, CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.ExecuteAsync(
            @"INSERT INTO channel_snapshots (channel_id, subscriber_count, total_view_count, recorded_at)
              VALUES (@ChannelId, @SubscriberCount, @TotalViewCount, @RecordedAt::date)
              ON CONFLICT (channel_id, recorded_at) DO UPDATE SET
                  subscriber_count = EXCLUDED.subscriber_count,
                  total_view_count = EXCLUDED.total_view_count",
            new
            {
                snapshot.ChannelId,
                snapshot.SubscriberCount,
                snapshot.TotalViewCount,
                RecordedAt = snapshot.RecordedAt.Date
            });
    }

    private class SnapshotRow
    {
        public long id { get; set; }
        public string channel_id { get; set; } = string.Empty;
        public long subscriber_count { get; set; }
        public long total_view_count { get; set; }
        public DateTime recorded_at { get; set; }
    }
}
