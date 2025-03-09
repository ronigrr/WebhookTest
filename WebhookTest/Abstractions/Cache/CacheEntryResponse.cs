using System.ComponentModel.DataAnnotations;

namespace WebhookTest.Abstractions.Cache;

//Data Transfer Object
public class CacheEntryResponse(string id,string? webhookUrl, double remainingTime) : IComparable<CacheEntryResponse>
{
    public string Id { get; } = id;
    public string? WebhookUrl { get; } = webhookUrl;
    public double RemainingTime { get; } = remainingTime;
    
    public int CompareTo(CacheEntryResponse? other)
    {
        return other == null ? 1 : RemainingTime.CompareTo(other.RemainingTime);
    }
}