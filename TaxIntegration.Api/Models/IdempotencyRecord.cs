namespace TaxIntegration.Api.Models;

public class IdempotencyRecord
{
    public string Key { get; set; } = "";
    public Guid OrderId { get; set; }
    public string ResponseBody { get; set; } = "";
    public int ResponseStatus { get; set; }
    public DateTime CreatedAt { get; set; }
}
