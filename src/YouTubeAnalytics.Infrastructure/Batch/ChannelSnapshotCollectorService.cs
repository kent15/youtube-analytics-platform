using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using YouTubeAnalytics.Application.Interfaces;
using YouTubeAnalytics.Infrastructure.Configuration;

namespace YouTubeAnalytics.Infrastructure.Batch;

public class ChannelSnapshotCollectorService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly BatchConfigStore _configStore;
    private readonly ILogger<ChannelSnapshotCollectorService> _logger;

    public ChannelSnapshotCollectorService(
        IServiceScopeFactory scopeFactory,
        BatchConfigStore configStore,
        ILogger<ChannelSnapshotCollectorService> logger)
    {
        _scopeFactory = scopeFactory;
        _configStore = configStore;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ChannelSnapshotCollectorService started");

        while (!stoppingToken.IsCancellationRequested)
        {
            var delay = CalculateDelayUntilNextExecution();
            _logger.LogInformation("Next batch execution in {Delay} at {NextRun}",
                delay, DateTime.Now.Add(delay).ToString("yyyy-MM-dd HH:mm"));

            try
            {
                await Task.Delay(delay, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            await ExecuteBatchAsync(stoppingToken);
        }

        _logger.LogInformation("ChannelSnapshotCollectorService stopped");
    }

    private TimeSpan CalculateDelayUntilNextExecution()
    {
        var config = _configStore.GetConfig();
        if (!TimeOnly.TryParse(config.ExecutionTime, out var executionTime))
            executionTime = new TimeOnly(3, 0);

        var now = DateTime.Now;
        var todayExecution = now.Date.Add(executionTime.ToTimeSpan());

        var nextExecution = now < todayExecution ? todayExecution : todayExecution.AddDays(1);
        return nextExecution - now;
    }

    private async Task ExecuteBatchAsync(CancellationToken stoppingToken)
    {
        var config = _configStore.GetConfig();
        if (!config.Enabled)
        {
            _logger.LogInformation("Batch is disabled, skipping execution");
            return;
        }

        var channels = config.Channels;
        if (channels.Count == 0)
        {
            _logger.LogInformation("No channels configured for tracking, skipping execution");
            return;
        }

        _logger.LogInformation("Starting batch snapshot collection for {Count} channels", channels.Count);

        var successCount = 0;
        var failCount = 0;

        foreach (var entry in channels)
        {
            if (stoppingToken.IsCancellationRequested) break;

            try
            {
                using var scope = _scopeFactory.CreateScope();
                var analysisService = scope.ServiceProvider.GetRequiredService<IChannelAnalysisService>();

                await analysisService.AnalyzeChannelAsync(entry.ChannelId, stoppingToken);

                successCount++;
                _logger.LogInformation("Batch: collected snapshot for {Label} ({ChannelId})",
                    entry.Label, entry.ChannelId);
            }
            catch (Exception ex)
            {
                failCount++;
                _logger.LogError(ex, "Batch: failed to collect snapshot for {Label} ({ChannelId})",
                    entry.Label, entry.ChannelId);
            }
        }

        _logger.LogInformation("Batch completed: {Success} succeeded, {Failed} failed out of {Total}",
            successCount, failCount, channels.Count);
    }
}
