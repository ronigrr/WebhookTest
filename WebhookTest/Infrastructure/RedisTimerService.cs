using System.Runtime.Serialization.Json;
using System.Text.Json;
using StackExchange.Redis;
using WebhookTest.Abstractions.Cache;

namespace WebhookTest.Infrastructure;

public class RedisTimerService : IRedisTimerService
{
    private readonly ILogger<RedisTimerService> _logger;
    private readonly IDatabase _redis;

    public RedisTimerService(ILogger<RedisTimerService> logger, IConnectionMultiplexer redis)
    {
        _logger = logger;
        _redis = redis.GetDatabase();
    }
    
    // public RedisTimerService(ILogger<RedisTimerService> logger, IConfiguration configuration)
    // {
    //     _logger = logger;
    //     var redisConnection = ConnectionMultiplexer.Connect(configuration.GetConnectionString("Redis")!);
    //     _redis = redisConnection.GetDatabase();
    // }

    public async Task AddTimer(string id, double delayInSeconds, string webhookUrl)
    {
        var expiration = DateTimeOffset.UtcNow.AddSeconds(delayInSeconds).ToUnixTimeSeconds();
        var payload = JsonSerializer.Serialize(new TimerCacheEntry(id, webhookUrl));
        
        await _redis.SortedSetAddAsync("timers", id, expiration); // add in the correct sorted location
        await _redis.StringSetAsync($"timer:{id}", payload); 
    }

    public async Task<CacheEntryResponse> GetTimerById(string id)
    {
        var data = await _redis.StringGetAsync($"timer:{id}");
        if (data.HasValue == false)
        {
            return new CacheEntryResponse(id, null, 0);
        }

        var timerCacheEntry = JsonSerializer.Deserialize<TimerCacheEntry>(data!);
        var expirationTimestamp = await _redis.SortedSetScoreAsync("timers", id);
        if (expirationTimestamp.HasValue)
        {
            var remainingTime = expirationTimestamp.Value - DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            remainingTime = remainingTime > 0 ? remainingTime : 0;
            return new CacheEntryResponse(timerCacheEntry!.Id, timerCacheEntry.WebhookUrl, remainingTime);
        }
        
        return new CacheEntryResponse(timerCacheEntry!.Id, timerCacheEntry.WebhookUrl, 0);
    }

    public async IAsyncEnumerable<CacheEntryResponse> GetExpiredTimers(int batchSize)
    {
        var currentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
       var expiredTimers = await _redis.SortedSetRangeByScoreAsync("timers", Double.NegativeInfinity, currentTimestamp, take: batchSize);
        
        // var skip = 0;
        // var timers = await _redis.SortedSetRangeByScoreAsync("timers", Double.NegativeInfinity, currentTimestamp, take: batchSize,skip: skip);
        // do
        // {
        //     foreach (var timer in timers)
        //     {
        //         var data = await _redis.StringGetAsync($"timer:{timer}");
        //         if (data.HasValue == false) continue;
        //
        //         var payload = JsonSerializer.Deserialize<TimerCacheEntry>(data!);
        //         yield return new CacheEntryResponse(payload!.Id, payload.WebhookUrl, 0);
        //     }
        //     skip += batchSize;
        //
        //     timers = await _redis.SortedSetRangeByScoreAsync("timers", Double.NegativeInfinity, currentTimestamp,
        //         take: batchSize, skip: skip);
        // } while (timers.Length != 0);
        
        foreach (var expiredTimer in expiredTimers)
        {
            var data = await _redis.StringGetAsync($"timer:{expiredTimer}");
            if (data.HasValue == false) continue;
        
            var payload = JsonSerializer.Deserialize<TimerCacheEntry>(data!);
            yield return new CacheEntryResponse(payload!.Id, payload.WebhookUrl, 0);
        }
    }

    public async Task RemoveTimer(string id)
    {
        await _redis.SortedSetRemoveAsync("timers", id);
        await _redis.KeyDeleteAsync($"timer:{id}");
    }
}