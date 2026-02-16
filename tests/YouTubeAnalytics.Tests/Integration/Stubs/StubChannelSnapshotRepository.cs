using YouTubeAnalytics.Domain.Entities;
using YouTubeAnalytics.Domain.Repositories;

namespace YouTubeAnalytics.Tests.Integration.Stubs;

public class StubChannelSnapshotRepository : IChannelSnapshotRepository
{
    private readonly List<ChannelSnapshot> _snapshots = new();

    public Task<IReadOnlyList<ChannelSnapshot>> GetByChannelIdAsync(string channelId, int days, CancellationToken cancellationToken = default)
    {
        var cutoff = DateTime.UtcNow.AddDays(-days);
        var result = _snapshots
            .Where(s => s.ChannelId == channelId && s.RecordedAt >= cutoff)
            .ToList();
        return Task.FromResult<IReadOnlyList<ChannelSnapshot>>(result);
    }

    public Task UpsertAsync(ChannelSnapshot snapshot, CancellationToken cancellationToken = default)
    {
        _snapshots.RemoveAll(s => s.ChannelId == snapshot.ChannelId && s.RecordedAt.Date == snapshot.RecordedAt.Date);
        _snapshots.Add(snapshot);
        return Task.CompletedTask;
    }
}
