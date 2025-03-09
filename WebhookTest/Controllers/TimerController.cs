using Microsoft.AspNetCore.Mvc;
using WebhookTest.Abstractions.Cache;
using WebhookTest.Abstractions.Requests;
using WebhookTest.Abstractions.Responses;
using WebhookTest.Infrastructure;

namespace WebhookTest.Controllers;

[ApiController]
[Route("[controller]")]
public class TimerController(IRedisTimerService redisTimerService, ITimerControllerService timerControllerService) : ControllerBase
{
    [HttpPost("[action]")]
    public async Task<ActionResult<SetTimerResponse>> SetTimer([FromBody] SetTimerRequest request)
    {
        var response = await timerControllerService.AddTimer(request);

        return response == null ?
            StatusCode(500, "Failed to add timer after multiple retries. Please try again later.") 
            : Ok(response);
    }
    
    [HttpGet("[action]")]
    public async Task<ActionResult<GetTimerResponse>> GetTimer([FromQuery] GetTimerRequest request)
    {
        
        var response = await timerControllerService.GetTimer(request);
        
        return response == null ?
            StatusCode(500, "Failed to get timer. Please try again later.") 
            : Ok(response);
    }
    
    [HttpGet("[action]")]
    public async Task<List<CacheEntryResponse>> GetTimers()
    { 
        var response = await timerControllerService.GetTimers();
       return response;
    }
    
    [HttpDelete("[action]")]
    public async Task<ActionResult<DeleteTimerResponse>> DeleteTimer([FromQuery] DeleteTimerRequest deleterRequest)
    {
        var response = await timerControllerService.DeleteTimer(deleterRequest);
        return response == null ?
            StatusCode(500, "Failed to DeleteTimer timer. Please try again later.") 
            : Ok(response);
    }
}