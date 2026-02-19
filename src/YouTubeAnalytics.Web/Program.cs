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
    builder.Services.AddInfrastructure(builder.Configuration);
}

// Application Services
var recentDaysPeriod = builder.Configuration.GetValue<int>("AnalysisConfig:RecentDaysPeriod", 30);
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

app.MapGet("/api/quota", async (HttpContext context) =>
{
    var quotaManager = context.RequestServices.GetRequiredService<YouTubeAnalytics.Infrastructure.YouTube.QuotaManager>();
    var cancellationToken = context.RequestAborted;
    var remaining = await quotaManager.GetRemainingQuotaAsync(cancellationToken);
    return Results.Ok(new { remaining, limit = 10000 });
});

app.Run();

public partial class Program { }
