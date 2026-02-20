using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using YouTubeAnalytics.Application.Interfaces;
using YouTubeAnalytics.Domain.Repositories;
using YouTubeAnalytics.Domain.Services;
using YouTubeAnalytics.Infrastructure.Batch;
using YouTubeAnalytics.Infrastructure.Cache;
using YouTubeAnalytics.Infrastructure.Persistence.Repositories;
using YouTubeAnalytics.Infrastructure.YouTube;

namespace YouTubeAnalytics.Infrastructure.Configuration;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration, string? configFilePath = null)
    {
        var connectionString = configuration["DatabaseConfig:ConnectionString"]
            ?? throw new InvalidOperationException("DatabaseConfig:ConnectionString is required");

        // Repositories
        services.AddSingleton<IChannelRepository>(new ChannelRepository(connectionString));
        services.AddSingleton<IVideoRepository>(new VideoRepository(connectionString));
        services.AddSingleton<IChannelSnapshotRepository>(new ChannelSnapshotRepository(connectionString));

        // Redis
        var redisConnectionString = configuration["CacheConfig:RedisConnectionString"] ?? "localhost:6379";
        services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConnectionString));
        services.AddSingleton<ICacheService, CacheService>();

        // YouTube API
        var apiKey = configuration["YouTubeApi:ApiKey"]
            ?? throw new InvalidOperationException("YouTubeApi:ApiKey is required");
        var dailyQuotaLimit = configuration.GetValue<int>("YouTubeApi:DailyQuotaLimit", 10000);
        var alertThreshold = configuration.GetValue<int>("YouTubeApi:QuotaAlertThreshold", 8000);
        var rateLimitPerSecond = configuration.GetValue<int>("YouTubeApi:RateLimitPerSecond", 10);

        services.AddSingleton(sp => new QuotaManager(
            dailyQuotaLimit,
            alertThreshold,
            sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<QuotaManager>>()));

        services.AddSingleton(new RateLimiter(rateLimitPerSecond));

        services.AddSingleton<IYouTubeApiClient>(sp => new YouTubeApiClient(
            apiKey,
            sp.GetRequiredService<QuotaManager>(),
            sp.GetRequiredService<RateLimiter>(),
            sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<YouTubeApiClient>>()));

        // Domain Services
        var growthThreshold = configuration.GetValue<double>("AnalysisConfig:GrowthThresholdMultiplier", 1.2);
        services.AddSingleton(new GrowthJudgementService(growthThreshold));

        var highFreq = configuration.GetValue<int>("AnalysisConfig:PublishingFrequency:HighFrequencyPerWeek", 3);
        var medFreq = configuration.GetValue<int>("AnalysisConfig:PublishingFrequency:MediumFrequencyPerWeek", 1);
        var topPercent = configuration.GetValue<int>("AnalysisConfig:ViralDependency:TopPercent", 10);
        var shareThreshold = configuration.GetValue<int>("AnalysisConfig:ViralDependency:ShareThreshold", 50);
        services.AddSingleton(new PublishingPatternService(highFreq, medFreq, topPercent, shareThreshold));

        // Batch Config
        var batchConfig = new BatchConfig();
        configuration.GetSection("BatchConfig").Bind(batchConfig);
        var resolvedConfigPath = configFilePath ?? "appsettings.json";

        services.AddSingleton(sp => new BatchConfigStore(
            resolvedConfigPath,
            batchConfig,
            sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<BatchConfigStore>>()));

        services.AddHostedService<ChannelSnapshotCollectorService>();

        return services;
    }
}
