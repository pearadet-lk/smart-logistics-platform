using Microsoft.EntityFrameworkCore;
using SmartLogistics.Shared.Shipments;

namespace SmartLogistics.Tracking.Api.Data;

public sealed class TrackingDbContext(DbContextOptions<TrackingDbContext> options) : DbContext(options)
{
    public DbSet<ShipmentStatusHistory> ShipmentStatusHistories => Set<ShipmentStatusHistory>();
    public DbSet<ContainerTracking> ContainerTrackings => Set<ContainerTracking>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ShipmentStatusHistory>().HasIndex(x => x.ShipmentId);
        modelBuilder.Entity<ContainerTracking>().HasIndex(x => x.ShipmentId);
    }
}

public static class TrackingDbSeeder
{
    public static async Task SeedAsync(TrackingDbContext db)
    {
        var shipmentId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        if (await db.ShipmentStatusHistories.AnyAsync(h => h.ShipmentId == shipmentId)) return;

        foreach (var (status, location, daysAgo) in new[]
        {
            (ShipmentStatus.Booked, "BKK", 5),
            (ShipmentStatus.Loaded, "BKK Terminal", 4),
            (ShipmentStatus.InTransit, "Pacific Ocean", 2)
        })
        {
            db.ShipmentStatusHistories.Add(new ShipmentStatusHistory
            {
                Id = Guid.NewGuid(),
                ShipmentId = shipmentId,
                Status = status,
                Location = location,
                CreatedDate = DateTime.UtcNow.AddDays(-daysAgo)
            });
        }

        db.ContainerTrackings.Add(new ContainerTracking
        {
            Id = Guid.NewGuid(),
            ShipmentId = shipmentId,
            ContainerNo = "MSCU1234567",
            SealNo = "SL-998877",
            CurrentPort = "Pacific Ocean",
            Etd = DateTime.UtcNow.AddDays(-4),
            Eta = DateTime.UtcNow.AddDays(3)
        });

        await db.SaveChangesAsync();
    }
}
