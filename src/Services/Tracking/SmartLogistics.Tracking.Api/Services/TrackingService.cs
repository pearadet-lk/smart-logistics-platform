using Microsoft.EntityFrameworkCore;
using SmartLogistics.Shared.Shipments;
using SmartLogistics.Tracking.Api.Data;

namespace SmartLogistics.Tracking.Api.Services;

public sealed class TrackingService(TrackingDbContext db)
{
    public async Task RecordBookedAsync(Guid shipmentId, string location, CancellationToken ct = default)
    {
        if (await db.ShipmentStatusHistories.AnyAsync(h => h.ShipmentId == shipmentId, ct)) return;

        db.ShipmentStatusHistories.Add(new ShipmentStatusHistory
        {
            Id = Guid.NewGuid(),
            ShipmentId = shipmentId,
            Status = ShipmentStatus.Booked,
            Location = location,
            CreatedDate = DateTime.UtcNow
        });

        await db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<ShipmentStatusHistory>> GetTimelineAsync(Guid shipmentId, CancellationToken ct = default) =>
        await db.ShipmentStatusHistories
            .Where(h => h.ShipmentId == shipmentId)
            .OrderBy(h => h.CreatedDate)
            .AsNoTracking()
            .ToListAsync(ct);

    public async Task<IReadOnlyList<ContainerTracking>> GetContainersAsync(Guid shipmentId, CancellationToken ct = default) =>
        await db.ContainerTrackings
            .Where(c => c.ShipmentId == shipmentId)
            .AsNoTracking()
            .ToListAsync(ct);
}
