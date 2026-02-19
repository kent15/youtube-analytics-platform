using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace YouTubeAnalytics.Tests.Integration;

public class ApiEndpointTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ApiEndpointTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetHealth_ReturnsOkWithHealthCheckMessage()
    {
        var response = await _client.GetAsync("/api/health");

        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadAsStringAsync();
        Assert.Equal("YouTube Analytics Tool", body);
    }

    [Fact]
    public async Task GetQuota_ReturnsOkWithRemainingAndLimit()
    {
        var response = await _client.GetAsync("/api/quota");

        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(json.TryGetProperty("remaining", out _));
        Assert.True(json.TryGetProperty("limit", out var limit));
        Assert.Equal(10000, limit.GetInt32());
    }

    [Fact]
    public async Task GetAnalysis_ValidChannel_ReturnsOkWithAnalysisResult()
    {
        var response = await _client.GetAsync("/api/channels/UC_test/analysis");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();

        // チャンネル情報の検証
        var channel = json.GetProperty("channel");
        Assert.Equal("UC_test", channel.GetProperty("channelId").GetString());
        Assert.Equal("Test Channel", channel.GetProperty("channelName").GetString());
        Assert.Equal(10000, channel.GetProperty("subscriberCount").GetInt64());

        // 分析結果フィールドの存在を検証
        Assert.True(json.TryGetProperty("recentVideoCount", out _));
        Assert.True(json.TryGetProperty("averageViewCount", out _));
        Assert.True(json.TryGetProperty("growthTrend", out _));
        Assert.True(json.TryGetProperty("publishingFrequency", out _));
        Assert.True(json.TryGetProperty("contentStrategy", out _));
    }

    [Fact]
    public async Task GetAnalysis_ValidChannel_ReturnsRecentVideos()
    {
        var response = await _client.GetAsync("/api/channels/UC_test/analysis");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        var videos = json.GetProperty("recentVideos");
        Assert.Equal(3, videos.GetArrayLength());
    }

    [Fact]
    public async Task GetAnalysis_NotFoundChannel_Returns404()
    {
        var response = await _client.GetAsync("/api/channels/UC_nonexistent/analysis");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(json.TryGetProperty("error", out _));
    }
}
