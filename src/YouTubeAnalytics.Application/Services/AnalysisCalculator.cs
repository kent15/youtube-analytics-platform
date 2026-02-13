using YouTubeAnalytics.Domain.Entities;

namespace YouTubeAnalytics.Application.Services;

public static class AnalysisCalculator
{
    public static int CountRecentVideos(IReadOnlyList<Video> videos, int recentDays)
    {
        var cutoff = DateTime.UtcNow.AddDays(-recentDays);
        return videos.Count(v => v.PublishedAt >= cutoff);
    }

    public static double CalculateAverageViewCount(IReadOnlyList<Video> videos)
    {
        if (videos.Count == 0)
            return 0;

        return videos.Average(v => (double)v.ViewCount);
    }
}
