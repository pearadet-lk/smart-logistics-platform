using Microsoft.EntityFrameworkCore;
using SmartLogistics.Shared.Audit;
using SmartLogistics.Shared.Shipments;
using ShipmentEntity = SmartLogistics.Shared.Shipments.Shipment;

namespace SmartLogistics.Reporting.Api.Data;

public sealed class ShipmentReadContext(DbContextOptions<ShipmentReadContext> options) : DbContext(options)
{
    public DbSet<ShipmentEntity> Shipments => Set<ShipmentEntity>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ShipmentEntity>().ToTable("Shipments");
        modelBuilder.Entity<AuditLog>().ToTable("AuditLogs");
    }
}

public sealed class TariffReadContext(DbContextOptions<TariffReadContext> options) : DbContext(options)
{
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.Entity<AuditLog>().ToTable("AuditLogs");
}

public sealed class BillingReadContext(DbContextOptions<BillingReadContext> options) : DbContext(options)
{
    public DbSet<InvoiceDraftRow> InvoiceDrafts => Set<InvoiceDraftRow>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.Entity<InvoiceDraftRow>().ToTable("InvoiceDrafts");
}

public sealed class InvoiceDraftRow
{
    public Guid Id { get; set; }
    public Guid ShipmentId { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
}
