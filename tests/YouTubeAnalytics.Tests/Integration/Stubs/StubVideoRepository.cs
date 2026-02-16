using YouTubeAnalytics.Domain.Entities;
using YouTubeAnalytics.Domain.Repositories;

namespace YouTubeAnalytics.Tests.Integration.Stubs;

public class StubVideoRepository : IVideoRepository
{
    private readonly Dictionary<string, List<Video>> _videos = new();

    public Task<IReadOnlyList<Video>> GetByChannelIdAsync(string channelId, CancellationToken cancellationToken = default)
    {
        if (_videos.TryGetValue(channelId, out var videos))
            return Task.FromResult<IReadOnlyList<Video>>(videos);

        return Task.FromResult<IReadOnlyList<Video>>(Array.Empty<Video>());
    }

    public Task SaveManyAsync(IEnumerable<Video> videos, CancellationToken cancellationToken = default)
    {
        foreach (var video in videos)
        {
            if (!_videos.ContainsKey(video.ChannelId))
                _videos[video.ChannelId] = new List<Video>();
            _videos[video.ChannelId].Add(video);
        }
        return Task.CompletedTask;
    }
}
