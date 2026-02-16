using YouTubeAnalytics.Domain.Entities;
using YouTubeAnalytics.Domain.Repositories;

namespace YouTubeAnalytics.Tests.Integration.Stubs;

public class StubChannelRepository : IChannelRepository
{
    private readonly Dictionary<string, Channel> _channels = new();

    public Task<Channel?> GetByIdAsync(string channelId, CancellationToken cancellationToken = default)
    {
        _channels.TryGetValue(channelId, out var channel);
        return Task.FromResult(channel);
    }

    public Task SaveAsync(Channel channel, CancellationToken cancellationToken = default)
    {
        _channels[channel.ChannelId] = channel;
        return Task.CompletedTask;
    }
}
