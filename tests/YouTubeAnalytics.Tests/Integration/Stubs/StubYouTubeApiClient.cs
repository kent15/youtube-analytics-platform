using YouTubeAnalytics.Application.Interfaces;
using YouTubeAnalytics.Domain.Entities;

namespace YouTubeAnalytics.Tests.Integration.Stubs;

public class StubYouTubeApiClient : IYouTubeApiClient
{
    private readonly Dictionary<string, Channel> _channels = new();
    private readonly Dictionary<string, IReadOnlyList<Video>> _videos = new();

    public void AddChannel(Channel channel, IReadOnlyList<Video> videos)
    {
        _channels[channel.ChannelId] = channel;
        _videos[channel.UploadsPlaylistId] = videos;
    }

    public Task<Channel> GetChannelAsync(string channelId, CancellationToken cancellationToken = default)
    {
        if (_channels.TryGetValue(channelId, out var channel))
            return Task.FromResult(channel);

        throw new InvalidOperationException($"Channel {channelId} not found");
    }

    public Task<IReadOnlyList<Video>> GetRecentVideosAsync(string uploadsPlaylistId, CancellationToken cancellationToken = default)
    {
        if (_videos.TryGetValue(uploadsPlaylistId, out var videos))
            return Task.FromResult(videos);

        return Task.FromResult<IReadOnlyList<Video>>(Array.Empty<Video>());
    }
}
