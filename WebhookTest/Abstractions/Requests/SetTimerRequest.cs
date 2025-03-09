using System.ComponentModel.DataAnnotations;

namespace WebhookTest.Abstractions.Requests;

//Request DTO
public class SetTimerRequest
{
    public int Hours { get; set; }
    public int Minutes { get; set; }
    public int Seconds { get; set; }
    
    [Required]
    [Url(ErrorMessage = "The webhook URL is not valid.")]
    public string WebhookUrl { get; set; }
}