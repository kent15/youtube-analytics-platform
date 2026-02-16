using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using YouTubeAnalytics.Application.Interfaces;
using YouTubeAnalytics.Domain.Entities;
using YouTubeAnalytics.Domain.Repositories;
using YouTubeAnalytics.Domain.Services;
using YouTubeAnalytics.Infrastructure.YouTube;
using YouTubeAnalytics.Tests.Integration.Stubs;

namespace YouTubeAnalytics.Tests.Integration;

public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    public StubYouTubeApiClient StubYouTubeApi { get; } = new();

    public TestWebApplicationFactory()
    {
        var channel = new Channel("UC_test", "Test Channel", 10000, 500000, 200, "UU_test", DateTime.UtcNow);
        var videos = new List<Video>
        {
            new("V1", "UC_test", "Video 1", DateTime.UtcNow.AddDays(-2), 5000, 100, 20),
            new("V2", "UC_test", "Video 2", DateTime.UtcNow.AddDays(-5), 3000, 80, 15),
            new("V3", "UC_test", "Video 3", DateTime.UtcNow.AddDays(-10), 4000, 90, 18)
        };
        StubYouTubeApi.AddChannel(channel, videos);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // スタブで外部依存を差し替え
            services.AddSingleton<IYouTubeApiClient>(StubYouTubeApi);
            services.AddSingleton<ICacheService, StubCacheService>();
            services.AddSingleton<IChannelRepository, StubChannelRepository>();
            services.AddSingleton<IVideoRepository, StubVideoRepository>();
            services.AddSingleton<IChannelSnapshotRepository, StubChannelSnapshotRepository>();

            // ドメインサービス
            services.AddSingleton(new GrowthJudgementService(1.2));
            services.AddSingleton(new PublishingPatternService(3, 1, 10, 50));

            // QuotaManager (GET /api/quota で必要)
            services.AddSingleton(sp => new QuotaManager(
                10000, 8000,
                NullLogger<QuotaManager>.Instance));
        });
    }
}
