using Microsoft.AspNetCore.Mvc;
using WebhookTest.Abstractions.Requests;
using WebhookTest.Abstractions.Responses;
using WebhookTest.Infrastructure;

namespace WebhookTest.Controllers;

[ApiController]
[Route("[controller]")]
public class TimerController(IRedisTimerService redisTimerService) : ControllerBase
{
    [HttpPost("[action]")]
    public async Task<SetTimerResponse> SetTimer([FromBody] SetTimerRequest request)
    {
        var id = Guid.NewGuid().ToString();
        await redisTimerService.AddTimer(id,
            (request.Hours * 3600) + (request.Minutes * 60) + request.Seconds, request.WebhookUrl);

        return new SetTimerResponse(id);
    }
    
    [HttpGet("[action]")]
    public async Task<GetTimerResponse> GetTimer([FromQuery] GetTimerRequest request)
    {
        var timer = await redisTimerService.GetTimerById(request.Id);
        if (timer.WebhookUrl == null)
        {
            return new GetTimerResponse(request.Id, 0);
        }

        return new GetTimerResponse(request.Id, timer.RemainingTime);
    }
}