namespace TaxIntegration.Api.Models;

public class IntegrationEvent
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public string EventType { get; set; } = "";
    public string Status { get; set; } = "Pending";
    public int RetryCount { get; set; }
    public int MaxRetries { get; set; } = 3;
    public DateTime? LastAttemptAt { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; }
}
