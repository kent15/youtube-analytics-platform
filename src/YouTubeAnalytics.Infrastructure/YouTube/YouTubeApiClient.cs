using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Microsoft.Extensions.Logging;
using YouTubeAnalytics.Application.Interfaces;
using YouTubeAnalytics.Domain.Entities;

namespace YouTubeAnalytics.Infrastructure.YouTube;

public class YouTubeApiClient : IYouTubeApiClient
{
    private readonly YouTubeService _youtubeService;
    private readonly QuotaManager _quotaManager;
    private readonly RateLimiter _rateLimiter;
    private readonly ILogger<YouTubeApiClient> _logger;

    public YouTubeApiClient(
        string apiKey,
        QuotaManager quotaManager,
        RateLimiter rateLimiter,
        ILogger<YouTubeApiClient> logger)
    {
        _youtubeService = new YouTubeService(new BaseClientService.Initializer
        {
            ApiKey = apiKey,
            ApplicationName = "YouTubeAnalyticsTool"
        });
        _quotaManager = quotaManager;
        _rateLimiter = rateLimiter;
        _logger = logger;
    }

    public async Task<Channel> GetChannelAsync(string channelId, CancellationToken cancellationToken = default)
    {
        await _quotaManager.CheckAndReserveAsync(1, cancellationToken);
        await _rateLimiter.WaitForPermitAsync(cancellationToken);

        _logger.LogInformation("Fetching channel info for {ChannelId}", channelId);

        var request = _youtubeService.Channels.List("snippet,statistics,contentDetails");
        request.Id = channelId;

        var response = await request.ExecuteAsync(cancellationToken);
        var ch = response.Items?.FirstOrDefault()
            ?? throw new InvalidOperationException($"Channel not found: {channelId}");

        return new Channel(
            ch.Id,
            ch.Snippet.Title,
            (long)(ch.Statistics.SubscriberCount ?? 0),
            (long)(ch.Statistics.ViewCount ?? 0),
            (long)(ch.Statistics.VideoCount ?? 0),
            ch.ContentDetails.RelatedPlaylists.Uploads,
            DateTime.UtcNow);
    }

    public async Task<IReadOnlyList<Video>> GetRecentVideosAsync(string uploadsPlaylistId, CancellationToken cancellationToken = default)
    {
        await _quotaManager.CheckAndReserveAsync(1, cancellationToken);
        await _rateLimiter.WaitForPermitAsync(cancellationToken);

        _logger.LogInformation("Fetching playlist items for {PlaylistId}", uploadsPlaylistId);

        var playlistRequest = _youtubeService.PlaylistItems.List("contentDetails");
        playlistRequest.PlaylistId = uploadsPlaylistId;
        playlistRequest.MaxResults = 50;

        var playlistResponse = await playlistRequest.ExecuteAsync(cancellationToken);
        var videoIds = playlistResponse.Items?
            .Select(item => item.ContentDetails.VideoId)
            .ToList() ?? new List<string>();

        if (videoIds.Count == 0)
            return Array.Empty<Video>();

        await _quotaManager.CheckAndReserveAsync(1, cancellationToken);
        await _rateLimiter.WaitForPermitAsync(cancellationToken);

        _logger.LogInformation("Fetching details for {Count} videos", videoIds.Count);

        var videoRequest = _youtubeService.Videos.List("snippet,statistics,contentDetails");
        videoRequest.Id = string.Join(",", videoIds);

        var videoResponse = await videoRequest.ExecuteAsync(cancellationToken);

        var videos = new List<Video>();
        foreach (var v in videoResponse.Items ?? Enumerable.Empty<Google.Apis.YouTube.v3.Data.Video>())
        {
            var duration = v.ContentDetails?.Duration ?? string.Empty;
            if (IsShortVideo(duration))
                continue;

            var channelId = v.Snippet.ChannelId;
            videos.Add(new Video(
                v.Id,
                channelId,
                v.Snippet.Title,
                v.Snippet.PublishedAtDateTimeOffset?.UtcDateTime ?? DateTime.UtcNow,
                (long)(v.Statistics?.ViewCount ?? 0),
                (long)(v.Statistics?.LikeCount ?? 0),
                (long)(v.Statistics?.CommentCount ?? 0)));
        }

        return videos;
    }

    private static bool IsShortVideo(string isoDuration)
    {
        if (string.IsNullOrEmpty(isoDuration))
            return false;

        try
        {
            var duration = System.Xml.XmlConvert.ToTimeSpan(isoDuration);
            return duration.TotalSeconds <= 60;
        }
        catch
        {
            return false;
        }
    }
}
