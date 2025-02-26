namespace WebhookTest.Abstractions.Cache;

//DTO
public class TimerCacheEntry(string id, string webhookUrl)
{
    public string Id { get; } = id;
    public string WebhookUrl { get; } = webhookUrl;
}