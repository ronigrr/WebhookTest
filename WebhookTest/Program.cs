using Polly;
using StackExchange.Redis;
using WebhookTest.Infrastructure;
using WebhookTest.Services;

//basic web application builder
var builder = WebApplication.CreateBuilder(args);

//register swagger service
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//builder.Services.AddSingleton<IRedisTimerService, RedisTimerService>();
//Dependency injection of redis
builder.Services.AddSingleton<IRedisTimerService>(provider =>
{
    
    var configuration = provider.GetRequiredService<IConfiguration>();
    var logger = provider.GetRequiredService<ILogger<RedisTimerService>>();
    
    var redisConnectionString = builder.Configuration.GetConnectionString("Redis")!;
    
    var redisConfiguration = ConfigurationOptions.Parse(redisConnectionString);
    redisConfiguration.AbortOnConnectFail = configuration.GetValue<bool>("RedisSettings:AbortOnConnectFail");
    redisConfiguration.ConnectRetry = configuration.GetValue<int>("RedisSettings:ConnectRetry");
    redisConfiguration.ReconnectRetryPolicy = new ExponentialRetry(configuration.GetValue<int>("RedisSettings:ReconnectRetryDelayMs"));
    redisConfiguration.ConnectTimeout = 15000;
    redisConfiguration.SyncTimeout = 15000;
    
    var redisConnection =  Policy.Handle<RedisConnectionException>()
        .WaitAndRetry(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
            (exception, timeSpan, retryCount, context) =>
            {
                logger.LogWarning($"Redis connection failed. Waiting {timeSpan} before retry {retryCount}.");
            })
        .Execute(() => ConnectionMultiplexer.Connect(redisConfiguration));
    
    redisConnection.ConnectionFailed += async (sender, args) =>
    {
        logger.LogWarning($"Redis connection lost: {args.EndPoint}. Retrying...");

        // Retry logic: Wait and try to reconnect
        await Task.Run(() =>
        {
            Policy.Handle<RedisConnectionException>()
                .WaitAndRetryForever(
                    retryAttempt => TimeSpan.FromSeconds(Math.Min(30, Math.Pow(2, retryAttempt))),
                    (exception, timeSpan) =>
                    {
                        logger.LogWarning($"Redis reconnection attempt failed. Retrying in {timeSpan.TotalSeconds} seconds...");
                    })
                .Execute(() => redisConnection = ConnectionMultiplexer.Connect(redisConfiguration));
        });
    };

    redisConnection.ConnectionRestored += (sender, args) =>
    {
        logger.LogInformation($"Redis connection restored: {args.EndPoint}");
    };
    
    return new RedisTimerService(logger, redisConnection);
});

builder.Services.AddSingleton<ITimerControllerService, TimerControllerService>();

//Added our hosted background service as a background worker 
builder.Services.AddHostedService<TimerBackgroundService>();

//Added http-service so the app can make outgoing http requests 
builder.Services.AddHttpClient();

builder.Services.AddControllers();

//Creates all the dependency's as a service
//Builds the application with all registered services & middleware.
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//Redirects HTTP requests to HTTPS for security.
app.UseHttpsRedirection();

//Enables authentication & authorization (though no authentication is configured yet).
app.UseAuthorization();

//maps the dependency so we can use them
app.MapControllers();

await app.RunAsync();
