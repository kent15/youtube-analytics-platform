using YouTubeAnalytics.Application.DTOs;

namespace YouTubeAnalytics.Application.Interfaces;

public interface IVideoRankingService
{
    Task<VideoRankingResultDto> GetRankingAsync(
        string sortBy,
        int periodDays,
        int limit,
        CancellationToken cancellationToken = default);
}
