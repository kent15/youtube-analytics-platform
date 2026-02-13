using YouTubeAnalytics.Domain.Entities;
using YouTubeAnalytics.Domain.Enums;

namespace YouTubeAnalytics.Domain.Services;

public class PublishingPatternService
{
    private readonly int _highFrequencyPerWeek;
    private readonly int _mediumFrequencyPerWeek;
    private readonly int _topPercent;
    private readonly int _shareThreshold;

    public PublishingPatternService(
        int highFrequencyPerWeek,
        int mediumFrequencyPerWeek,
        int topPercent,
        int shareThreshold)
    {
        _highFrequencyPerWeek = highFrequencyPerWeek;
        _mediumFrequencyPerWeek = mediumFrequencyPerWeek;
        _topPercent = topPercent;
        _shareThreshold = shareThreshold;
    }

    public PublishingFrequency JudgeFrequency(IReadOnlyList<Video> recentVideos)
    {
        if (recentVideos.Count == 0)
            return PublishingFrequency.Low;

        var earliest = recentVideos.Min(v => v.PublishedAt);
        var latest = recentVideos.Max(v => v.PublishedAt);
        var weeks = Math.Max((latest - earliest).TotalDays / 7.0, 1.0);
        var videosPerWeek = recentVideos.Count / weeks;

        if (videosPerWeek >= _highFrequencyPerWeek)
            return PublishingFrequency.High;

        if (videosPerWeek >= _mediumFrequencyPerWeek)
            return PublishingFrequency.Medium;

        return PublishingFrequency.Low;
    }

    public ContentStrategy JudgeContentStrategy(IReadOnlyList<Video> recentVideos)
    {
        if (recentVideos.Count == 0)
            return ContentStrategy.Stable;

        var totalViews = recentVideos.Sum(v => v.ViewCount);
        if (totalViews == 0)
            return ContentStrategy.Stable;

        var topCount = Math.Max((int)Math.Ceiling(recentVideos.Count * _topPercent / 100.0), 1);
        var topViews = recentVideos
            .OrderByDescending(v => v.ViewCount)
            .Take(topCount)
            .Sum(v => v.ViewCount);

        var topSharePercent = (double)topViews / totalViews * 100;

        return topSharePercent >= _shareThreshold
            ? ContentStrategy.ViralDependent
            : ContentStrategy.Stable;
    }
}
