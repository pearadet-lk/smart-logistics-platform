using Microsoft.EntityFrameworkCore;
using SmartLogistics.Shared.Audit;
using SmartLogistics.Shared.Outbox;
using ShipmentEntity = SmartLogistics.Shared.Shipments.Shipment;
using SmartLogistics.Shipment.Api.Data;

namespace SmartLogistics.Shipment.Api.Data;

public sealed class ShipmentDbContext(DbContextOptions<ShipmentDbContext> options) : DbContext(options)
{
    public DbSet<ShipmentEntity> Shipments => Set<ShipmentEntity>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ShipmentEntity>().HasIndex(x => x.TenantId);
        modelBuilder.Entity<OutboxMessage>().HasIndex(x => x.Status);
    }
}

public static class ShipmentDbSeeder
{
    public static async Task SeedAsync(ShipmentDbContext db)
    {
        if (await db.Shipments.AnyAsync()) return;

        db.Shipments.Add(new ShipmentEntity
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            TenantId = "DHL",
            CustomerId = "CUST-001",
            OriginPort = "BKK",
            DestinationPort = "LAX",
            Status = SmartLogistics.Shared.Shipments.ShipmentStatus.InTransit,
            CreatedDate = DateTime.UtcNow.AddDays(-5)
        });

        await db.SaveChangesAsync();
    }
}
