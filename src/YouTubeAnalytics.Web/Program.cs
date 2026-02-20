using Serilog;
using YouTubeAnalytics.Application.Interfaces;
using YouTubeAnalytics.Application.Services;
using YouTubeAnalytics.Infrastructure.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Serilog
builder.Host.UseSerilog((context, config) =>
    config.ReadFrom.Configuration(context.Configuration));

// Infrastructure (Repositories, Cache, YouTube API, Domain Services)
if (!builder.Environment.IsEnvironment("Testing"))
{
    var configFilePath = Path.Combine(builder.Environment.ContentRootPath, "appsettings.json");
    builder.Services.AddInfrastructure(builder.Configuration, configFilePath);
}

// Application Services
var recentDaysPeriod = builder.Configuration.GetValue<int>("AnalysisConfig:RecentDaysPeriod", 30);
builder.Services.AddScoped<IVideoRankingService>(sp =>
    new VideoRankingService(
        sp.GetRequiredService<YouTubeAnalytics.Domain.Repositories.IVideoRepository>()));

builder.Services.AddScoped<IChannelAnalysisService>(sp =>
    new ChannelAnalysisService(
        sp.GetRequiredService<IYouTubeApiClient>(),
        sp.GetRequiredService<ICacheService>(),
        sp.GetRequiredService<YouTubeAnalytics.Domain.Repositories.IChannelRepository>(),
        sp.GetRequiredService<YouTubeAnalytics.Domain.Repositories.IVideoRepository>(),
        sp.GetRequiredService<YouTubeAnalytics.Domain.Repositories.IChannelSnapshotRepository>(),
        sp.GetRequiredService<YouTubeAnalytics.Domain.Services.GrowthJudgementService>(),
        sp.GetRequiredService<YouTubeAnalytics.Domain.Services.PublishingPatternService>(),
        sp.GetRequiredService<ILogger<ChannelAnalysisService>>(),
        recentDaysPeriod));

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/api/health", () => "YouTube Analytics Tool");

app.MapGet("/api/channels/{channelId}/analysis", async (
    string channelId,
    HttpContext context) =>
{
    var analysisService = context.RequestServices.GetRequiredService<IChannelAnalysisService>();
    var cancellationToken = context.RequestAborted;
    try
    {
        var result = await analysisService.AnalyzeChannelAsync(channelId, cancellationToken);
        return Results.Ok(result);
    }
    catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
    {
        return Results.NotFound(new { error = ex.Message });
    }
    catch (InvalidOperationException ex) when (ex.Message.Contains("quota"))
    {
        return Results.Json(
            new { error = "YouTube APIの日次クォータ上限に達しました。太平洋時間の午前0時にリセットされます。" },
            statusCode: 429);
    }
});

app.MapGet("/api/videos/ranking", async (
    HttpContext context,
    string? sortBy,
    int? period,
    int? limit) =>
{
    var rankingService = context.RequestServices.GetRequiredService<IVideoRankingService>();
    var cancellationToken = context.RequestAborted;
    var result = await rankingService.GetRankingAsync(
        sortBy ?? "viewCount",
        period ?? 30,
        limit ?? 50,
        cancellationToken);
    return Results.Ok(result);
});

app.MapGet("/api/quota", async (HttpContext context) =>
{
    var quotaManager = context.RequestServices.GetRequiredService<YouTubeAnalytics.Infrastructure.YouTube.QuotaManager>();
    var cancellationToken = context.RequestAborted;
    var remaining = await quotaManager.GetRemainingQuotaAsync(cancellationToken);
    return Results.Ok(new { remaining, limit = 10000 });
});

// Tracking channels API
app.MapGet("/api/tracking/channels", (HttpContext context) =>
{
    var store = context.RequestServices.GetRequiredService<BatchConfigStore>();
    var channels = store.GetChannels();
    return Results.Ok(channels);
});

app.MapGet("/api/tracking/channels/{channelId}/status", (string channelId, HttpContext context) =>
{
    var store = context.RequestServices.GetRequiredService<BatchConfigStore>();
    return Results.Ok(new { tracked = store.IsTracked(channelId) });
});

app.MapPost("/api/tracking/channels", (AddTrackingChannelRequest req, HttpContext context) =>
{
    if (string.IsNullOrWhiteSpace(req.ChannelId))
        return Results.BadRequest(new { error = "ChannelId is required" });

    var store = context.RequestServices.GetRequiredService<BatchConfigStore>();
    if (store.IsTracked(req.ChannelId))
        return Results.Conflict(new { error = "Channel is already tracked" });

    store.AddChannel(req.ChannelId, req.Label ?? req.ChannelId);
    return Results.Created($"/api/tracking/channels/{req.ChannelId}/status", new { channelId = req.ChannelId, label = req.Label });
});

app.MapDelete("/api/tracking/channels/{channelId}", (string channelId, HttpContext context) =>
{
    var store = context.RequestServices.GetRequiredService<BatchConfigStore>();
    if (!store.RemoveChannel(channelId))
        return Results.NotFound(new { error = "Channel not found in tracking list" });

    return Results.Ok(new { removed = channelId });
});

app.Run();

public partial class Program { }

record AddTrackingChannelRequest(string ChannelId, string Label);
