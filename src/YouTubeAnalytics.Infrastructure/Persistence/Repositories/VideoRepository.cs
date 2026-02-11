using YouTubeAnalytics.Domain.Entities;
using YouTubeAnalytics.Domain.Repositories;

namespace YouTubeAnalytics.Infrastructure.Persistence.Repositories;

public class VideoRepository : IVideoRepository
{
    public Task<IReadOnlyList<Video>> GetByChannelIdAsync(string channelId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task SaveManyAsync(IEnumerable<Video> videos, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
