using System.ComponentModel.DataAnnotations;

namespace WebhookTest.Abstractions.Cache;

//Data Transfer Object
public class CacheEntryResponse(string id,[property: Url] string? webhookUrl, double remainingTime)
{
    public string Id { get; } = id;
    public string? WebhookUrl { get; } = webhookUrl;
    public double RemainingTime { get; } = remainingTime;
}