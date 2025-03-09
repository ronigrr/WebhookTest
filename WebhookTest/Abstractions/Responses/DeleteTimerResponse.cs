namespace WebhookTest.Abstractions.Responses;

public class DeleteTimerResponse(string id)
{
    public string Id { get; set; } = id;
}