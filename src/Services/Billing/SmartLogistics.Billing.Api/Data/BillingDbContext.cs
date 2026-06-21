using Microsoft.EntityFrameworkCore;
using SmartLogistics.Shared.Billing;

namespace SmartLogistics.Billing.Api.Data;

public sealed class BillingDbContext(DbContextOptions<BillingDbContext> options) : DbContext(options)
{
    public DbSet<InvoiceDraft> InvoiceDrafts => Set<InvoiceDraft>();
    public DbSet<ProcessedEvent> ProcessedEvents => Set<ProcessedEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProcessedEvent>().HasIndex(x => x.EventId).IsUnique();
    }
}

public sealed class InvoiceDraft
{
    public Guid Id { get; set; }
    public Guid ShipmentId { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Status { get; set; } = "Draft";
    public DateTime CreatedDate { get; set; }
}

public sealed class ProcessedEvent
{
    public Guid Id { get; set; }
    public string EventId { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
    public DateTime ProcessedDate { get; set; }
}

public static class BillingDbSeeder
{
    public static Task SeedAsync(BillingDbContext _) => Task.CompletedTask;
}
