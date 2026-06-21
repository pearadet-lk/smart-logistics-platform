namespace SmartLogistics.Shared.Outbox;

public enum OutboxStatus
{
    Pending,
    Published,
    Failed
}

public sealed class OutboxMessage
{
    public Guid Id { get; set; }
    public string Topic { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public OutboxStatus Status { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? ProcessedDate { get; set; }
}
