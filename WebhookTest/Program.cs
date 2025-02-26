using StackExchange.Redis;
using WebhookTest.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//builder.Services.AddSingleton<IRedisTimerService, RedisTimerService>();
builder.Services.AddSingleton<IRedisTimerService>(provider =>
{
    var redisConnection = ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis")!);
    var logger = provider.GetRequiredService<ILogger<RedisTimerService>>();
    return new RedisTimerService(logger, redisConnection);
});

builder.Services.AddHostedService<TimerBackgroundService>();

builder.Services.AddHttpClient();

builder.Services.AddControllers();
//Creates all the dependensic as a service

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
//maps the dependensic so i can use them

await app.RunAsync();
