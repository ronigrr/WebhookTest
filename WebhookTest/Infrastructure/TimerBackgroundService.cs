namespace WebhookTest.Infrastructure;

public class TimerBackgroundService : BackgroundService
{
    private readonly ILogger<TimerBackgroundService> _logger;
    private readonly IRedisTimerService _redisTimerService;
    private readonly IHttpClientFactory _httpClientFactory;

    public TimerBackgroundService(ILogger<TimerBackgroundService> logger, IRedisTimerService redisTimerService, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _redisTimerService = redisTimerService;
        _httpClientFactory = httpClientFactory;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var expiredTimers = _redisTimerService.GetExpiredTimers(100);

            await foreach (var expiredTimer in expiredTimers)
            {
                try
                {
                    var httpClient = _httpClientFactory.CreateClient();
                    var response = await httpClient.PostAsync(expiredTimer.WebhookUrl, new StringContent(""), stoppingToken);

                    if (response.IsSuccessStatusCode)
                    {
                        await _redisTimerService.RemoveTimer(expiredTimer.Id);
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"An error has occurred while processing expired timer with id {expiredTimer.Id}", expiredTimer);
                }
            }
            
            await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
        }
    }
}