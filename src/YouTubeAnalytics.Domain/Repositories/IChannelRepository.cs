using YouTubeAnalytics.Domain.Entities;

namespace YouTubeAnalytics.Domain.Repositories;

public interface IChannelRepository
{
    Task<Channel?> GetByIdAsync(string channelId, CancellationToken cancellationToken = default);
    Task SaveAsync(Channel channel, CancellationToken cancellationToken = default);
}
