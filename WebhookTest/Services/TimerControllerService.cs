using Microsoft.AspNetCore.Mvc;
using Polly;
using StackExchange.Redis;
using WebhookTest.Abstractions.Cache;
using WebhookTest.Abstractions.Requests;
using WebhookTest.Abstractions.Responses;
using WebhookTest.Infrastructure;


namespace WebhookTest.Services;

public class TimerControllerService (IRedisTimerService redisTimerService) :ITimerControllerService
{
    public async Task<SetTimerResponse?> AddTimer(SetTimerRequest request)
    {
        var id = Guid.NewGuid().ToString();
        var delayInSeconds = (request.Hours * 3600) + (request.Minutes * 60) + request.Seconds;
        
        var retryPolicy = Policy
            .Handle<RedisConnectionException>() 
            .Or<TimeoutException>()
            .WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), 
                (exception, timeSpan, retryCount, context) =>
                {
                    Console.WriteLine($"Retry {retryCount} after {timeSpan.TotalSeconds} seconds due to: {exception.Message}");
                });
       
        try
        {
            await retryPolicy.ExecuteAsync(async () =>
            {
                await redisTimerService.AddTimer(id, delayInSeconds, request.WebhookUrl);
            });

            return new SetTimerResponse(id); // Return success if the timer was added
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to add timer after multiple retries: {ex.Message}");
            return null;
        }
    }

    public async Task<GetTimerResponse?> GetTimer(GetTimerRequest request)
    {
        var timer = await redisTimerService.GetTimerById(request.Id);
        
        return timer.WebhookUrl == null ?
            new GetTimerResponse(request.Id, 0) 
            : new GetTimerResponse(request.Id, timer.RemainingTime);
    }

    public async Task<List<CacheEntryResponse>> GetTimers()
    {
        var timers = await redisTimerService.GetTimers();
        timers.Sort();
        return timers;
    }

    public async Task<DeleteTimerResponse?> DeleteTimer(DeleteTimerRequest deleteTimerRequest)
    {
        var result = await redisTimerService.RemoveTimer(deleteTimerRequest.Id);
        return result ? new DeleteTimerResponse(deleteTimerRequest.Id) : null;
    }
}