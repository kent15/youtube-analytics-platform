using YouTubeAnalytics.Domain.Entities;

namespace YouTubeAnalytics.Domain.Repositories;

public interface IChannelSnapshotRepository
{
    Task<IReadOnlyList<ChannelSnapshot>> GetByChannelIdAsync(string channelId, int days, CancellationToken cancellationToken = default);
    Task UpsertAsync(ChannelSnapshot snapshot, CancellationToken cancellationToken = default);
}
