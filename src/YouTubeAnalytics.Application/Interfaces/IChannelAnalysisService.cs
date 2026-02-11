using YouTubeAnalytics.Application.DTOs;

namespace YouTubeAnalytics.Application.Interfaces;

public interface IChannelAnalysisService
{
    Task<AnalysisResultDto> AnalyzeChannelAsync(string channelId, CancellationToken cancellationToken = default);
}
