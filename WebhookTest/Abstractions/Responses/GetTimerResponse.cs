namespace WebhookTest.Abstractions.Responses;

//inner dto
public class GetTimerResponse(string id, double remainingTime)
{
    public string Id { get; } = id;
    public double RemainingTime { get; } = remainingTime;
}