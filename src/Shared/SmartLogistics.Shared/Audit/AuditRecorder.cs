using Microsoft.EntityFrameworkCore;

namespace SmartLogistics.Shared.Audit;

public static class AuditRecorder
{
    public static AuditLog Create(
        string userId,
        string action,
        string entity,
        string? oldValue = null,
        string? newValue = null,
        string? ipAddress = null,
        string? traceId = null) =>
        new()
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Action = action,
            Entity = entity,
            OldValue = oldValue,
            NewValue = newValue,
            IpAddress = ipAddress,
            TraceId = traceId,
            CreatedDate = DateTime.UtcNow
        };

    public static void Record(
        DbSet<AuditLog> logs,
        string userId,
        string action,
        string entity,
        string? oldValue = null,
        string? newValue = null,
        string? ipAddress = null,
        string? traceId = null) =>
        logs.Add(Create(userId, action, entity, oldValue, newValue, ipAddress, traceId));
}
