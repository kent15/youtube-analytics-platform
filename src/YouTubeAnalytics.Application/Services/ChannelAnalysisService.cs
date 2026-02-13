using Microsoft.Extensions.Logging;
using YouTubeAnalytics.Application.DTOs;
using YouTubeAnalytics.Application.Interfaces;
using YouTubeAnalytics.Domain.Entities;
using YouTubeAnalytics.Domain.Repositories;
using YouTubeAnalytics.Domain.Services;

namespace YouTubeAnalytics.Application.Services;

public class ChannelAnalysisService : IChannelAnalysisService
{
    private readonly IYouTubeApiClient _youtubeApiClient;
    private readonly ICacheService _cacheService;
    private readonly IChannelRepository _channelRepository;
    private readonly IVideoRepository _videoRepository;
    private readonly IChannelSnapshotRepository _snapshotRepository;
    private readonly GrowthJudgementService _growthJudgementService;
    private readonly PublishingPatternService _publishingPatternService;
    private readonly ILogger<ChannelAnalysisService> _logger;
    private readonly int _recentDaysPeriod;

    public ChannelAnalysisService(
        IYouTubeApiClient youtubeApiClient,
        ICacheService cacheService,
        IChannelRepository channelRepository,
        IVideoRepository videoRepository,
        IChannelSnapshotRepository snapshotRepository,
        GrowthJudgementService growthJudgementService,
        PublishingPatternService publishingPatternService,
        ILogger<ChannelAnalysisService> logger,
        int recentDaysPeriod = 30)
    {
        _youtubeApiClient = youtubeApiClient;
        _cacheService = cacheService;
        _channelRepository = channelRepository;
        _videoRepository = videoRepository;
        _snapshotRepository = snapshotRepository;
        _growthJudgementService = growthJudgementService;
        _publishingPatternService = publishingPatternService;
        _logger = logger;
        _recentDaysPeriod = recentDaysPeriod;
    }

    public async Task<AnalysisResultDto> AnalyzeChannelAsync(string channelId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting analysis for channel {ChannelId}", channelId);

        var cacheKey = $"analysis:{channelId}";
        var cached = await _cacheService.GetAsync<AnalysisResultDto>(cacheKey, cancellationToken);
        if (cached != null)
        {
            _logger.LogInformation("Returning cached analysis for channel {ChannelId}", channelId);
            return cached;
        }

        var channel = await _youtubeApiClient.GetChannelAsync(channelId, cancellationToken);
        await _channelRepository.SaveAsync(channel, cancellationToken);

        var snapshot = new ChannelSnapshot(
            0,
            channel.ChannelId,
            channel.SubscriberCount,
            channel.TotalViewCount,
            DateTime.UtcNow);
        await _snapshotRepository.UpsertAsync(snapshot, cancellationToken);

        var videos = await _youtubeApiClient.GetRecentVideosAsync(channel.UploadsPlaylistId, cancellationToken);
        await _videoRepository.SaveManyAsync(videos, cancellationToken);

        var cutoff = DateTime.UtcNow.AddDays(-_recentDaysPeriod);
        var recentVideos = videos.Where(v => v.PublishedAt >= cutoff).ToList();

        var recentVideoCount = AnalysisCalculator.CountRecentVideos(videos, _recentDaysPeriod);
        var averageViewCount = AnalysisCalculator.CalculateAverageViewCount(recentVideos);

        var growthTrend = _growthJudgementService.Judge(channel, recentVideos);
        var publishingFrequency = _publishingPatternService.JudgeFrequency(recentVideos);
        var contentStrategy = _publishingPatternService.JudgeContentStrategy(recentVideos);

        var snapshots = await _snapshotRepository.GetByChannelIdAsync(channelId, 90, cancellationToken);

        var result = new AnalysisResultDto
        {
            Channel = new ChannelInfoDto
            {
                ChannelId = channel.ChannelId,
                ChannelName = channel.ChannelName,
                SubscriberCount = channel.SubscriberCount,
                TotalViewCount = channel.TotalViewCount,
                VideoCount = channel.VideoCount
            },
            RecentVideos = videos.Select(v => new VideoDetailDto
            {
                VideoId = v.VideoId,
                Title = v.Title,
                ThumbnailUrl = $"https://i.ytimg.com/vi/{v.VideoId}/mqdefault.jpg",
                PublishedAt = v.PublishedAt,
                ViewCount = v.ViewCount,
                LikeCount = v.LikeCount,
                CommentCount = v.CommentCount
            }).ToList(),
            RecentVideoCount = recentVideoCount,
            AverageViewCount = averageViewCount,
            GrowthTrend = growthTrend.ToString(),
            PublishingFrequency = publishingFrequency.ToString(),
            ContentStrategy = contentStrategy.ToString(),
            Snapshots = snapshots.Select(s => new ChannelSnapshotDto
            {
                RecordedAt = s.RecordedAt,
                SubscriberCount = s.SubscriberCount,
                TotalViewCount = s.TotalViewCount
            }).ToList()
        };

        await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromHours(6), cancellationToken);

        _logger.LogInformation("Completed analysis for channel {ChannelId}: {GrowthTrend}, {Frequency}, {Strategy}",
            channelId, growthTrend, publishingFrequency, contentStrategy);

        return result;
    }
}
