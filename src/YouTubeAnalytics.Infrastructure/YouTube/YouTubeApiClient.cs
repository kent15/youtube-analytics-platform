using YouTubeAnalytics.Domain.Entities;

namespace YouTubeAnalytics.Infrastructure.YouTube;

public class YouTubeApiClient
{
    public Task<Channel> GetChannelAsync(string channelId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<Video>> GetRecentVideosAsync(string uploadsPlaylistId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
