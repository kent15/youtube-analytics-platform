using YouTubeAnalytics.Domain.Entities;

namespace YouTubeAnalytics.Domain.Repositories;

public interface IVideoRepository
{
    Task<IReadOnlyList<Video>> GetByChannelIdAsync(string channelId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Video>> GetAllAsync(CancellationToken cancellationToken = default);
    Task SaveManyAsync(IEnumerable<Video> videos, CancellationToken cancellationToken = default);
}
