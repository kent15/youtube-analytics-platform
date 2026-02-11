using YouTubeAnalytics.Domain.Entities;
using YouTubeAnalytics.Domain.Enums;

namespace YouTubeAnalytics.Domain.Services;

public class PublishingPatternService
{
    public PublishingFrequency JudgeFrequency(IReadOnlyList<Video> recentVideos)
    {
        throw new NotImplementedException();
    }

    public ContentStrategy JudgeContentStrategy(IReadOnlyList<Video> recentVideos)
    {
        throw new NotImplementedException();
    }
}
