using YouTubeAnalytics.Application.DTOs;
using YouTubeAnalytics.Application.Interfaces;

namespace YouTubeAnalytics.Application.Services;

public class ChannelAnalysisService : IChannelAnalysisService
{
    public Task<AnalysisResultDto> AnalyzeChannelAsync(string channelId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
