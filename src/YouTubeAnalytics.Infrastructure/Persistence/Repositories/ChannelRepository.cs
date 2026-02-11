using YouTubeAnalytics.Domain.Entities;
using YouTubeAnalytics.Domain.Repositories;

namespace YouTubeAnalytics.Infrastructure.Persistence.Repositories;

public class ChannelRepository : IChannelRepository
{
    public Task<Channel?> GetByIdAsync(string channelId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task SaveAsync(Channel channel, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
