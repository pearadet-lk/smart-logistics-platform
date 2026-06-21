using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using SmartLogistics.Shared.Audit;
using SmartLogistics.Shared.Lookups;
using SmartLogistics.Shared.Tariffs;
using SmartLogistics.Shared.Workflow;
using SmartLogistics.Tariff.Api.Data;

namespace SmartLogistics.Tariff.Api.Services;

public sealed class WorkflowService(TariffDbContext db, TariffService tariffService)
{
    public async Task<WorkflowInstance?> GetForEntityAsync(string entityType, Guid entityId, CancellationToken ct = default) =>
        await db.WorkflowInstances.AsNoTracking()
            .FirstOrDefaultAsync(w => w.EntityType == entityType && w.EntityId == entityId, ct);

    public async Task<WorkflowInstance> StartTariffWorkflowAsync(TariffVersion version, string actor, CancellationToken ct = default)
    {
        var workflow = new WorkflowInstance
        {
            Id = Guid.NewGuid(),
            EntityType = "TariffVersion",
            EntityId = version.Id,
            Status = WorkflowStatus.Draft,
            TenantId = version.TenantId,
            CreatedBy = actor,
            CreatedDate = DateTime.UtcNow
        };

        db.WorkflowInstances.Add(workflow);
        db.WorkflowSteps.Add(new WorkflowStep
        {
            Id = Guid.NewGuid(),
            WorkflowInstanceId = workflow.Id,
            StepName = "Draft",
            Status = WorkflowStatus.Draft,
            Order = 1,
            CompletedDate = DateTime.UtcNow
        });

        await db.SaveChangesAsync(ct);
        return workflow;
    }

    public async Task<(WorkflowInstance?, TariffVersion?)> AdvanceAsync(
        Guid tariffVersionId,
        WorkflowStatus targetStatus,
        string actor,
        string? comment,
        string? ipAddress,
        string? traceId,
        CancellationToken ct = default)
    {
        var version = await db.TariffVersions.FindAsync([tariffVersionId], ct);
        if (version is null) return (null, null);

        var workflow = await db.WorkflowInstances
            .FirstOrDefaultAsync(w => w.EntityType == "TariffVersion" && w.EntityId == tariffVersionId, ct);

        if (workflow is null)
        {
            workflow = await StartTariffWorkflowAsync(version, actor, ct);
        }

        var oldStatus = workflow.Status.ToString();
        workflow.Status = targetStatus;
        var tariffStatus = targetStatus switch
        {
            WorkflowStatus.Submitted => TariffWorkflowStatus.Submitted,
            WorkflowStatus.Approved => TariffWorkflowStatus.Approved,
            WorkflowStatus.Published => TariffWorkflowStatus.Published,
            WorkflowStatus.Rejected => TariffWorkflowStatus.Draft,
            _ => version.Status
        };

        version.Status = tariffStatus;

        db.WorkflowSteps.Add(new WorkflowStep
        {
            Id = Guid.NewGuid(),
            WorkflowInstanceId = workflow.Id,
            StepName = targetStatus.ToString(),
            Status = targetStatus,
            Order = await db.WorkflowSteps.CountAsync(s => s.WorkflowInstanceId == workflow.Id, ct) + 1,
            CompletedDate = DateTime.UtcNow
        });

        db.ApprovalHistories.Add(new ApprovalHistory
        {
            Id = Guid.NewGuid(),
            WorkflowInstanceId = workflow.Id,
            Action = targetStatus.ToString(),
            Actor = actor,
            Comment = comment,
            CreatedDate = DateTime.UtcNow
        });

        AuditRecorder.Record(db.AuditLogs, actor, targetStatus.ToString(), "TariffWorkflow",
            oldValue: oldStatus, newValue: targetStatus.ToString(),
            ipAddress: ipAddress, traceId: traceId);

        await db.SaveChangesAsync(ct);
        await tariffService.InvalidateCacheAsync(version.TenantId, ct);

        return (workflow, version);
    }

    public async Task<IReadOnlyList<ApprovalHistory>> GetHistoryAsync(Guid workflowInstanceId, CancellationToken ct = default) =>
        await db.ApprovalHistories
            .Where(h => h.WorkflowInstanceId == workflowInstanceId)
            .OrderByDescending(h => h.CreatedDate)
            .AsNoTracking()
            .ToListAsync(ct);
}

public sealed class LookupCacheService(IDistributedCache cache)
{
    private static readonly TimeSpan Ttl = TimeSpan.FromHours(24);

    public async Task<IReadOnlyList<PortInfo>> GetPortsAsync(CancellationToken ct = default)
    {
        const string key = "lookups:ports";
        var cached = await cache.GetStringAsync(key, ct);
        if (cached is not null) return JsonSerializer.Deserialize<List<PortInfo>>(cached) ?? [];

        var ports = PortCountryLookups.Ports.ToList();
        await cache.SetStringAsync(key, JsonSerializer.Serialize(ports),
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = Ttl }, ct);
        return ports;
    }

    public async Task<IReadOnlyList<CountryInfo>> GetCountriesAsync(CancellationToken ct = default)
    {
        const string key = "lookups:countries";
        var cached = await cache.GetStringAsync(key, ct);
        if (cached is not null) return JsonSerializer.Deserialize<List<CountryInfo>>(cached) ?? [];

        var countries = PortCountryLookups.Countries.ToList();
        await cache.SetStringAsync(key, JsonSerializer.Serialize(countries),
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = Ttl }, ct);
        return countries;
    }
}
