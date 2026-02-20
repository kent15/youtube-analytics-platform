using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace YouTubeAnalytics.Infrastructure.Configuration;

public class BatchConfigStore
{
    private readonly string _configFilePath;
    private readonly ILogger<BatchConfigStore> _logger;
    private readonly object _lock = new();
    private BatchConfig _config;

    public BatchConfigStore(string configFilePath, BatchConfig initialConfig, ILogger<BatchConfigStore> logger)
    {
        _configFilePath = configFilePath;
        _config = initialConfig;
        _logger = logger;
    }

    public BatchConfig GetConfig()
    {
        lock (_lock)
        {
            return _config;
        }
    }

    public IReadOnlyList<BatchChannelEntry> GetChannels()
    {
        lock (_lock)
        {
            return _config.Channels.AsReadOnly();
        }
    }

    public bool IsTracked(string channelId)
    {
        lock (_lock)
        {
            return _config.Channels.Any(c => c.ChannelId == channelId);
        }
    }

    public void AddChannel(string channelId, string label)
    {
        lock (_lock)
        {
            if (_config.Channels.Any(c => c.ChannelId == channelId))
                return;

            _config.Channels.Add(new BatchChannelEntry { ChannelId = channelId, Label = label });
        }
        PersistAsync().ConfigureAwait(false);
    }

    public bool RemoveChannel(string channelId)
    {
        bool removed;
        lock (_lock)
        {
            removed = _config.Channels.RemoveAll(c => c.ChannelId == channelId) > 0;
        }
        if (removed)
            PersistAsync().ConfigureAwait(false);
        return removed;
    }

    private async Task PersistAsync()
    {
        try
        {
            var fullJson = await File.ReadAllTextAsync(_configFilePath);
            using var doc = JsonDocument.Parse(fullJson);
            var root = doc.RootElement;

            using var ms = new MemoryStream();
            using (var writer = new Utf8JsonWriter(ms, new JsonWriterOptions { Indented = true }))
            {
                writer.WriteStartObject();
                foreach (var prop in root.EnumerateObject())
                {
                    if (prop.Name == "BatchConfig")
                    {
                        writer.WritePropertyName("BatchConfig");
                        WriteBatchConfig(writer);
                    }
                    else
                    {
                        prop.WriteTo(writer);
                    }
                }
                writer.WriteEndObject();
            }

            var json = System.Text.Encoding.UTF8.GetString(ms.ToArray());
            await File.WriteAllTextAsync(_configFilePath, json);
            _logger.LogInformation("BatchConfig persisted to {Path}", _configFilePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to persist BatchConfig to {Path}", _configFilePath);
        }
    }

    private void WriteBatchConfig(Utf8JsonWriter writer)
    {
        lock (_lock)
        {
            writer.WriteStartObject();
            writer.WriteBoolean("Enabled", _config.Enabled);
            writer.WriteString("ExecutionTime", _config.ExecutionTime);
            writer.WriteStartArray("Channels");
            foreach (var ch in _config.Channels)
            {
                writer.WriteStartObject();
                writer.WriteString("ChannelId", ch.ChannelId);
                writer.WriteString("Label", ch.Label);
                writer.WriteEndObject();
            }
            writer.WriteEndArray();
            writer.WriteEndObject();
        }
    }
}
