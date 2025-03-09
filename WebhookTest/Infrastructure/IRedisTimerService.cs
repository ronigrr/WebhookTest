using StackExchange.Redis;
using WebhookTest.Abstractions.Cache;

namespace WebhookTest.Infrastructure;

public interface IRedisTimerService
{
    Task AddTimer(string id, double delayInSeconds, string webhookUrl);
    Task<CacheEntryResponse> GetTimerById(string id);
    IAsyncEnumerable<CacheEntryResponse> GetExpiredTimers(int batchSize);
    Task<bool> RemoveTimer(string id);
    Task<List<CacheEntryResponse>> GetTimers();
}