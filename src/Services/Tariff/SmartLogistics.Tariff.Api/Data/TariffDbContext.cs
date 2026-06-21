using Microsoft.EntityFrameworkCore;
using SmartLogistics.Shared.Audit;
using SmartLogistics.Shared.Tariffs;
using SmartLogistics.Shared.Workflow;

namespace SmartLogistics.Tariff.Api.Data;

public sealed class TariffDbContext(DbContextOptions<TariffDbContext> options) : DbContext(options)
{
    public DbSet<TariffVersion> TariffVersions => Set<TariffVersion>();
    public DbSet<ExchangeRate> ExchangeRates => Set<ExchangeRate>();
    public DbSet<WorkflowInstance> WorkflowInstances => Set<WorkflowInstance>();
    public DbSet<WorkflowStep> WorkflowSteps => Set<WorkflowStep>();
    public DbSet<ApprovalHistory> ApprovalHistories => Set<ApprovalHistory>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TariffVersion>().HasIndex(x => new { x.TenantId, x.VersionNo });
        modelBuilder.Entity<ExchangeRate>().HasIndex(x => new { x.BaseCurrency, x.QuoteCurrency, x.EffectiveDate });
        modelBuilder.Entity<WorkflowStep>()
            .HasOne<WorkflowInstance>()
            .WithMany()
            .HasForeignKey(x => x.WorkflowInstanceId);
        modelBuilder.Entity<ApprovalHistory>()
            .HasOne<WorkflowInstance>()
            .WithMany()
            .HasForeignKey(x => x.WorkflowInstanceId);
    }
}

public static class TariffDbSeeder
{
    public static async Task SeedAsync(TariffDbContext db)
    {
        if (await db.TariffVersions.AnyAsync()) return;

        var versionId = Guid.NewGuid();
        db.TariffVersions.Add(new TariffVersion
        {
            Id = versionId,
            VersionNo = 1,
            EffectiveDate = DateTime.UtcNow.AddMonths(-1),
            CreatedBy = "admin",
            CreatedDate = DateTime.UtcNow.AddMonths(-1),
            Status = TariffWorkflowStatus.Published,
            TenantId = "DHL",
            ContentJson = """{"routes":[{"origin":"BKK","destination":"LAX","rate":1200}]}"""
        });

        var workflowId = Guid.NewGuid();
        db.WorkflowInstances.Add(new WorkflowInstance
        {
            Id = workflowId,
            EntityType = "TariffVersion",
            EntityId = versionId,
            Status = WorkflowStatus.Published,
            TenantId = "DHL",
            CreatedBy = "admin",
            CreatedDate = DateTime.UtcNow.AddMonths(-1)
        });

        db.WorkflowSteps.AddRange(
            new WorkflowStep { Id = Guid.NewGuid(), WorkflowInstanceId = workflowId, StepName = "Draft", Status = WorkflowStatus.Draft, Order = 1, CompletedDate = DateTime.UtcNow.AddMonths(-1) },
            new WorkflowStep { Id = Guid.NewGuid(), WorkflowInstanceId = workflowId, StepName = "Submitted", Status = WorkflowStatus.Submitted, Order = 2, CompletedDate = DateTime.UtcNow.AddMonths(-1).AddDays(1) },
            new WorkflowStep { Id = Guid.NewGuid(), WorkflowInstanceId = workflowId, StepName = "Approved", Status = WorkflowStatus.Approved, Order = 3, CompletedDate = DateTime.UtcNow.AddMonths(-1).AddDays(2) },
            new WorkflowStep { Id = Guid.NewGuid(), WorkflowInstanceId = workflowId, StepName = "Published", Status = WorkflowStatus.Published, Order = 4, CompletedDate = DateTime.UtcNow.AddMonths(-1).AddDays(3) });

        db.ExchangeRates.AddRange(
            new ExchangeRate { Id = Guid.NewGuid(), BaseCurrency = "USD", QuoteCurrency = "THB", Rate = 36.5m, EffectiveDate = DateTime.UtcNow.Date, ImportedAt = DateTime.UtcNow },
            new ExchangeRate { Id = Guid.NewGuid(), BaseCurrency = "USD", QuoteCurrency = "EUR", Rate = 0.92m, EffectiveDate = DateTime.UtcNow.Date, ImportedAt = DateTime.UtcNow });

        await db.SaveChangesAsync();
    }
}
