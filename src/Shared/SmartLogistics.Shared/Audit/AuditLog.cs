namespace SmartLogistics.Shared.Audit;

public sealed class AuditLog
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Entity { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string? IpAddress { get; set; }
    public string? TraceId { get; set; }
    public DateTime CreatedDate { get; set; }
}
