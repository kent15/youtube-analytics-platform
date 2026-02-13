using YouTubeAnalytics.Domain.Entities;
using YouTubeAnalytics.Domain.Enums;

namespace YouTubeAnalytics.Domain.Services;

public class GrowthJudgementService
{
    private readonly double _growthThresholdMultiplier;

    public GrowthJudgementService(double growthThresholdMultiplier)
    {
        _growthThresholdMultiplier = growthThresholdMultiplier;
    }

    public GrowthTrend Judge(Channel channel, IReadOnlyList<Video> recentVideos)
    {
        if (recentVideos.Count == 0)
            return GrowthTrend.Declining;

        if (channel.VideoCount == 0 || channel.TotalViewCount == 0)
            return GrowthTrend.Stable;

        var channelAverageViews = (double)channel.TotalViewCount / channel.VideoCount;
        var recentAverageViews = recentVideos.Average(v => (double)v.ViewCount);

        if (recentAverageViews >= channelAverageViews * _growthThresholdMultiplier)
            return GrowthTrend.Growing;

        if (recentAverageViews < channelAverageViews / _growthThresholdMultiplier)
            return GrowthTrend.Declining;

        return GrowthTrend.Stable;
    }
}
