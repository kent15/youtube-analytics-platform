using YouTubeAnalytics.Domain.Entities;

namespace YouTubeAnalytics.Application.Interfaces;

public interface IYouTubeApiClient
{
    Task<Channel> GetChannelAsync(string channelId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Video>> GetRecentVideosAsync(string uploadsPlaylistId, CancellationToken cancellationToken = default);
}
