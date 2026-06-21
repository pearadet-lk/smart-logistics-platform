using Microsoft.EntityFrameworkCore;
using SmartLogistics.Shared.Audit;
using SmartLogistics.Shared.Outbox;
using ShipmentEntity = SmartLogistics.Shared.Shipments.Shipment;
using SmartLogistics.Shared.Shipments;
using SmartLogistics.Shipment.Api.Data;

namespace SmartLogistics.Shipment.Api.Services;

public sealed class ShipmentService(ShipmentDbContext db)
{
    public async Task<IReadOnlyList<ShipmentEntity>> GetAllAsync(string? tenantId, CancellationToken ct = default) =>
        await db.Shipments
            .Where(s => tenantId == null || s.TenantId == tenantId)
            .AsNoTracking()
            .ToListAsync(ct);

    public async Task<ShipmentEntity?> GetByIdAsync(Guid id, string? tenantId, CancellationToken ct = default) =>
        await db.Shipments
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id && (tenantId == null || s.TenantId == tenantId), ct);

    public async Task<ShipmentEntity> CreateAsync(
        ShipmentEntity shipment,
        string userId,
        string? ipAddress = null,
        string? traceId = null,
        CancellationToken ct = default)
    {
        shipment.Id = Guid.NewGuid();
        shipment.CreatedDate = DateTime.UtcNow;
        shipment.Status = ShipmentStatus.Booked;
        db.Shipments.Add(shipment);

        AuditRecorder.Record(db.AuditLogs, userId, "Create", "Shipment",
            newValue: $"{shipment.Id}|{shipment.OriginPort}→{shipment.DestinationPort}",
            ipAddress: ipAddress, traceId: traceId);

        await db.SaveChangesAsync(ct);
        return shipment;
    }

    public async Task<ShipmentEntity?> UpdateAsync(
        Guid id,
        string? tenantId,
        string userId,
        string? originPort,
        string? destinationPort,
        string? ipAddress,
        string? traceId,
        CancellationToken ct = default)
    {
        var shipment = await db.Shipments.FirstOrDefaultAsync(
            s => s.Id == id && (tenantId == null || s.TenantId == tenantId), ct);
        if (shipment is null) return null;

        var old = $"{shipment.OriginPort}→{shipment.DestinationPort}";
        if (!string.IsNullOrWhiteSpace(originPort)) shipment.OriginPort = originPort;
        if (!string.IsNullOrWhiteSpace(destinationPort)) shipment.DestinationPort = destinationPort;
        var updated = $"{shipment.OriginPort}→{shipment.DestinationPort}";

        AuditRecorder.Record(db.AuditLogs, userId, "Update", "Shipment", old, updated, ipAddress, traceId);
        await db.SaveChangesAsync(ct);
        return shipment;
    }

    public async Task<bool> DeleteAsync(
        Guid id,
        string? tenantId,
        string userId,
        string? ipAddress,
        string? traceId,
        CancellationToken ct = default)
    {
        var shipment = await db.Shipments.FirstOrDefaultAsync(
            s => s.Id == id && (tenantId == null || s.TenantId == tenantId), ct);
        if (shipment is null) return false;

        AuditRecorder.Record(db.AuditLogs, userId, "Delete", "Shipment",
            oldValue: shipment.Id.ToString(), ipAddress: ipAddress, traceId: traceId);

        db.Shipments.Remove(shipment);
        await db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<OutboxMessage> EnqueueOutboxAsync(string topic, string payload, CancellationToken ct = default)
    {
        var message = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Topic = topic,
            Payload = payload,
            Status = OutboxStatus.Pending,
            CreatedDate = DateTime.UtcNow
        };
        db.OutboxMessages.Add(message);
        await db.SaveChangesAsync(ct);
        return message;
    }

    public async Task<IReadOnlyList<OutboxMessage>> GetPendingOutboxAsync(CancellationToken ct = default) =>
        await db.OutboxMessages
            .Where(m => m.Status == OutboxStatus.Pending)
            .OrderBy(m => m.CreatedDate)
            .ToListAsync(ct);

    public async Task MarkOutboxPublishedAsync(Guid id, CancellationToken ct = default)
    {
        var message = await db.OutboxMessages.FindAsync([id], ct);
        if (message is null) return;
        message.Status = OutboxStatus.Published;
        message.ProcessedDate = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
    }

    public async Task MarkOutboxFailedAsync(Guid id, CancellationToken ct = default)
    {
        var message = await db.OutboxMessages.FindAsync([id], ct);
        if (message is null) return;
        message.Status = OutboxStatus.Failed;
        message.ProcessedDate = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
    }
}
