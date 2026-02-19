using YouTubeAnalytics.Application.DTOs;
using YouTubeAnalytics.Application.Interfaces;
using YouTubeAnalytics.Domain.Entities;
using YouTubeAnalytics.Domain.Repositories;

namespace YouTubeAnalytics.Application.Services;

public class VideoRankingService : IVideoRankingService
{
    private readonly IVideoRepository _videoRepository;

    private static readonly HashSet<string> ValidSortFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "viewCount", "likeCount", "commentCount", "likeRate"
    };

    public VideoRankingService(IVideoRepository videoRepository)
    {
        _videoRepository = videoRepository;
    }

    public async Task<VideoRankingResultDto> GetRankingAsync(
        string sortBy,
        int periodDays,
        int limit,
        CancellationToken cancellationToken = default)
    {
        if (!ValidSortFields.Contains(sortBy))
            sortBy = "viewCount";

        if (limit <= 0 || limit > 100)
            limit = 50;

        var allVideos = await _videoRepository.GetAllAsync(cancellationToken);

        var filtered = FilterByPeriod(allVideos, periodDays);
        var sorted = SortVideos(filtered, sortBy);
        var ranked = sorted.Take(limit).ToList();

        var totalViewCount = filtered.Sum(v => v.ViewCount);
        var totalLikeCount = filtered.Sum(v => v.LikeCount);
        var avgViewCount = filtered.Count > 0 ? totalViewCount / filtered.Count : 0;
        var avgLikeRate = totalViewCount > 0
            ? Math.Round((double)totalLikeCount / totalViewCount * 100, 1)
            : 0.0;

        return new VideoRankingResultDto
        {
            TotalCount = filtered.Count,
            TotalViewCount = totalViewCount,
            AverageViewCount = avgViewCount,
            AverageLikeRate = avgLikeRate,
            Items = ranked.Select((v, i) => new VideoRankingItemDto
            {
                Rank = i + 1,
                VideoId = v.VideoId,
                Title = v.Title,
                ChannelId = v.ChannelId,
                ThumbnailUrl = $"https://i.ytimg.com/vi/{v.VideoId}/mqdefault.jpg",
                PublishedAt = v.PublishedAt,
                ViewCount = v.ViewCount,
                LikeCount = v.LikeCount,
                CommentCount = v.CommentCount,
                LikeRate = v.ViewCount > 0
                    ? Math.Round((double)v.LikeCount / v.ViewCount * 100, 1)
                    : 0.0
            }).ToList()
        };
    }

    public static IReadOnlyList<Video> FilterByPeriod(IReadOnlyList<Video> videos, int periodDays)
    {
        if (periodDays <= 0)
            return videos;

        var cutoff = DateTime.UtcNow.AddDays(-periodDays);
        return videos.Where(v => v.PublishedAt >= cutoff).ToList();
    }

    public static IEnumerable<Video> SortVideos(IReadOnlyList<Video> videos, string sortBy)
    {
        return sortBy.ToLowerInvariant() switch
        {
            "likecount" => videos.OrderByDescending(v => v.LikeCount),
            "commentcount" => videos.OrderByDescending(v => v.CommentCount),
            "likerate" => videos.OrderByDescending(v =>
                v.ViewCount > 0 ? (double)v.LikeCount / v.ViewCount : 0.0),
            _ => videos.OrderByDescending(v => v.ViewCount),
        };
    }
}
