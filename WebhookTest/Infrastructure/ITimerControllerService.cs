using Microsoft.AspNetCore.Mvc;
using WebhookTest.Abstractions.Cache;
using WebhookTest.Abstractions.Requests;
using WebhookTest.Abstractions.Responses;

namespace WebhookTest.Infrastructure;

public interface ITimerControllerService
{
    Task<SetTimerResponse?> AddTimer(SetTimerRequest request);
    Task<GetTimerResponse?> GetTimer(GetTimerRequest request);
    Task<List<CacheEntryResponse>> GetTimers();
    Task<DeleteTimerResponse?> DeleteTimer(DeleteTimerRequest request);
}