using WebhookTest.Abstractions.Cache;

namespace WebhookTest.Infrastructure;

public interface IRedisTimerService
{
    Task AddTimer(string id, double delayInSeconds, string webhookUrl);
    Task<CacheEntryResponse> GetTimerById(string id);
    IAsyncEnumerable<CacheEntryResponse> GetExpiredTimers(int batchSize);
    Task RemoveTimer(string id);
}