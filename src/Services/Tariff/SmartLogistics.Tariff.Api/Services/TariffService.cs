using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using SmartLogistics.Shared.Audit;
using SmartLogistics.Shared.Tariffs;
using SmartLogistics.Tariff.Api.Data;

namespace SmartLogistics.Tariff.Api.Services;

public sealed class TariffService(TariffDbContext db, IDistributedCache cache)
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(15);

    public async Task<IReadOnlyList<TariffVersion>> GetAllAsync(string? tenantId, CancellationToken ct = default)
    {
        var cacheKey = $"tariffs:{tenantId ?? "all"}";
        var cached = await cache.GetStringAsync(cacheKey, ct);
        if (cached is not null)
        {
            return JsonSerializer.Deserialize<List<TariffVersion>>(cached) ?? [];
        }

        var items = await db.TariffVersions
            .Where(v => tenantId == null || v.TenantId == tenantId)
            .OrderByDescending(v => v.VersionNo)
            .AsNoTracking()
            .ToListAsync(ct);

        await cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(items),
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = CacheTtl }, ct);

        return items;
    }

    public async Task InvalidateCacheAsync(string tenantId, CancellationToken ct = default) =>
        await cache.RemoveAsync($"tariffs:{tenantId}", ct);

    public async Task<TariffVersion> CreateDraftAsync(
        string createdBy,
        string tenantId,
        string contentJson,
        string? ipAddress,
        string? traceId,
        CancellationToken ct = default)
    {
        var nextVersion = await db.TariffVersions
            .Where(v => v.TenantId == tenantId)
            .Select(v => v.VersionNo)
            .DefaultIfEmpty(0)
            .MaxAsync(ct) + 1;

        var version = new TariffVersion
        {
            Id = Guid.NewGuid(),
            VersionNo = nextVersion,
            EffectiveDate = DateTime.UtcNow,
            CreatedBy = createdBy,
            CreatedDate = DateTime.UtcNow,
            Status = TariffWorkflowStatus.Draft,
            TenantId = tenantId,
            ContentJson = contentJson
        };

        db.TariffVersions.Add(version);
        AuditRecorder.Record(db.AuditLogs, createdBy, "Create", "TariffVersion",
            newValue: $"v{nextVersion}|{version.Id}", ipAddress: ipAddress, traceId: traceId);

        await db.SaveChangesAsync(ct);
        await InvalidateCacheAsync(tenantId, ct);
        return version;
    }

    public async Task<TariffVersion?> AdvanceWorkflowAsync(
        Guid id,
        TariffWorkflowStatus status,
        string actor,
        string? ipAddress,
        string? traceId,
        CancellationToken ct = default)
    {
        var version = await db.TariffVersions.FindAsync([id], ct);
        if (version is null) return null;

        var oldStatus = version.Status.ToString();
        version.Status = status;

        AuditRecorder.Record(db.AuditLogs, actor, status.ToString(), "TariffVersion",
            oldValue: oldStatus, newValue: status.ToString(), ipAddress: ipAddress, traceId: traceId);

        await db.SaveChangesAsync(ct);
        await InvalidateCacheAsync(version.TenantId, ct);
        return version;
    }

    public async Task<object?> CompareAsync(Guid v1, Guid v2, CancellationToken ct = default)
    {
        var versions = await db.TariffVersions
            .Where(v => v.Id == v1 || v.Id == v2)
            .AsNoTracking()
            .ToListAsync(ct);

        var a = versions.FirstOrDefault(v => v.Id == v1);
        var b = versions.FirstOrDefault(v => v.Id == v2);
        if (a is null || b is null) return null;

        return new
        {
            version1 = a.VersionNo,
            version2 = b.VersionNo,
            diff = new { added = Array.Empty<string>(), modified = new[] { "rates" }, deleted = Array.Empty<string>() }
        };
    }

    public async Task<IReadOnlyList<ExchangeRate>> GetExchangeRatesAsync(CancellationToken ct = default)
    {
        var cacheKey = "exchange-rates:latest";
        var cached = await cache.GetStringAsync(cacheKey, ct);
        if (cached is not null)
        {
            return JsonSerializer.Deserialize<List<ExchangeRate>>(cached) ?? [];
        }

        var rates = await db.ExchangeRates
            .OrderByDescending(r => r.EffectiveDate)
            .Take(20)
            .AsNoTracking()
            .ToListAsync(ct);

        await cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(rates),
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = CacheTtl }, ct);

        return rates;
    }

    public async Task ImportExchangeRatesAsync(CancellationToken ct = default)
    {
        var today = DateTime.UtcNow.Date;
        var rates = new[]
        {
            new ExchangeRate { Id = Guid.NewGuid(), BaseCurrency = "USD", QuoteCurrency = "THB", Rate = 36.5m + Random.Shared.Next(0, 10) / 100m, EffectiveDate = today, ImportedAt = DateTime.UtcNow },
            new ExchangeRate { Id = Guid.NewGuid(), BaseCurrency = "USD", QuoteCurrency = "EUR", Rate = 0.92m, EffectiveDate = today, ImportedAt = DateTime.UtcNow },
            new ExchangeRate { Id = Guid.NewGuid(), BaseCurrency = "USD", QuoteCurrency = "GBP", Rate = 0.79m, EffectiveDate = today, ImportedAt = DateTime.UtcNow }
        };

        db.ExchangeRates.AddRange(rates);
        await db.SaveChangesAsync(ct);
        await cache.RemoveAsync("exchange-rates:latest", ct);
    }
}

public sealed class ExchangeRateImportService(IServiceScopeFactory scopeFactory, ILogger<ExchangeRateImportService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var tariff = scope.ServiceProvider.GetRequiredService<TariffService>();
                await tariff.ImportExchangeRatesAsync(stoppingToken);
                logger.LogInformation("Exchange rates imported at {Time}", DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Exchange rate import failed");
            }

            await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
        }
    }
}
