namespace SmartLogistics.Shared.Audit;

/// <summary>
/// Audit trail entity — capture Login, Logout, Create, Update, Delete.
/// TODO: Persist to each service database via EF Core.
/// </summary>
public sealed class AuditLog
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Entity { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public DateTime CreatedDate { get; set; }
}
