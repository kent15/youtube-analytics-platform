namespace YouTubeAnalytics.Infrastructure.YouTube;

public class QuotaManager
{
    public Task CheckAndReserveAsync(int requiredUnits, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<int> GetRemainingQuotaAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
