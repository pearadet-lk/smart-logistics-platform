using Microsoft.EntityFrameworkCore;
using SmartLogistics.Reporting.Api.Data;
using SmartLogistics.Shared.Shipments;

namespace SmartLogistics.Reporting.Api.Services;

public sealed class AnalyticsService(
    ShipmentReadContext shipments,
    BillingReadContext billing)
{
    public async Task<DashboardAnalytics> GetDashboardAsync(string? tenantId, CancellationToken ct = default)
    {
        var shipmentQuery = shipments.Shipments.AsNoTracking()
            .Where(s => tenantId == null || s.TenantId == tenantId);

        var total = await shipmentQuery.CountAsync(ct);
        var pending = await shipmentQuery.CountAsync(s => s.Status == ShipmentStatus.Booked, ct);
        var inTransit = await shipmentQuery.CountAsync(s => s.Status == ShipmentStatus.InTransit, ct);

        var byMonth = await shipmentQuery
            .GroupBy(s => new { s.CreatedDate.Year, s.CreatedDate.Month })
            .Select(g => new MonthlyCount(g.Key.Year, g.Key.Month, g.Count()))
            .OrderBy(x => x.Year).ThenBy(x => x.Month)
            .ToListAsync(ct);

        var topRoutes = await shipmentQuery
            .GroupBy(s => new { s.OriginPort, s.DestinationPort })
            .Select(g => new RouteCount(
                $"{g.Key.OriginPort} → {g.Key.DestinationPort}",
                g.Count()))
            .OrderByDescending(x => x.Count)
            .Take(5)
            .ToListAsync(ct);

        var topCustomers = await shipmentQuery
            .GroupBy(s => s.CustomerId)
            .Select(g => new CustomerCount(g.Key, g.Count()))
            .OrderByDescending(x => x.Count)
            .Take(5)
            .ToListAsync(ct);

        var revenueQuery = billing.InvoiceDrafts.AsNoTracking()
            .Where(i => tenantId == null || i.TenantId == tenantId);

        var revenueMtd = await revenueQuery
            .Where(i => i.CreatedDate.Month == DateTime.UtcNow.Month && i.CreatedDate.Year == DateTime.UtcNow.Year)
            .SumAsync(i => i.Amount, ct);

        var revenueByMonth = await revenueQuery
            .GroupBy(i => new { i.CreatedDate.Year, i.CreatedDate.Month })
            .Select(g => new MonthlyRevenue(g.Key.Year, g.Key.Month, g.Sum(x => x.Amount)))
            .OrderBy(x => x.Year).ThenBy(x => x.Month)
            .ToListAsync(ct);

        return new DashboardAnalytics(
            total,
            pending,
            inTransit,
            revenueMtd,
            PadMonthlyCounts(byMonth),
            PadMonthlyRevenue(revenueByMonth),
            topRoutes,
            topCustomers);
    }

    private static IReadOnlyList<MonthlyCount> PadMonthlyCounts(IReadOnlyList<MonthlyCount> data)
    {
        if (data.Count >= 6) return data.TakeLast(6).ToList();
        var now = DateTime.UtcNow;
        var result = new List<MonthlyCount>();
        for (var i = 5; i >= 0; i--)
        {
            var d = now.AddMonths(-i);
            var existing = data.FirstOrDefault(x => x.Year == d.Year && x.Month == d.Month);
            result.Add(existing ?? new MonthlyCount(d.Year, d.Month, 0));
        }
        return result;
    }

    private static IReadOnlyList<MonthlyRevenue> PadMonthlyRevenue(IReadOnlyList<MonthlyRevenue> data)
    {
        if (data.Count >= 6) return data.TakeLast(6).ToList();
        var now = DateTime.UtcNow;
        var result = new List<MonthlyRevenue>();
        for (var i = 5; i >= 0; i--)
        {
            var d = now.AddMonths(-i);
            var existing = data.FirstOrDefault(x => x.Year == d.Year && x.Month == d.Month);
            result.Add(existing ?? new MonthlyRevenue(d.Year, d.Month, 0));
        }
        return result;
    }
}

public sealed record DashboardAnalytics(
    int TotalShipments,
    int PendingShipments,
    int InTransitShipments,
    decimal RevenueMtd,
    IReadOnlyList<MonthlyCount> ShipmentsByMonth,
    IReadOnlyList<MonthlyRevenue> RevenueByMonth,
    IReadOnlyList<RouteCount> TopRoutes,
    IReadOnlyList<CustomerCount> TopCustomers);

public sealed record MonthlyCount(int Year, int Month, int Count);
public sealed record MonthlyRevenue(int Year, int Month, decimal Amount);
public sealed record RouteCount(string Route, int Count);
public sealed record CustomerCount(string CustomerId, int Count);

public sealed class AuditQueryService(ShipmentReadContext shipments, TariffReadContext tariffs)
{
    public async Task<IReadOnlyList<AuditLogDto>> GetAllAsync(int take, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 500);

        var shipmentLogs = await shipments.AuditLogs
            .OrderByDescending(x => x.CreatedDate)
            .Take(take)
            .AsNoTracking()
            .Select(x => new AuditLogDto(x.Id, x.UserId, x.Action, x.Entity, x.OldValue, x.NewValue, x.IpAddress, x.TraceId, x.CreatedDate, "Shipment"))
            .ToListAsync(ct);

        var tariffLogs = await tariffs.AuditLogs
            .OrderByDescending(x => x.CreatedDate)
            .Take(take)
            .AsNoTracking()
            .Select(x => new AuditLogDto(x.Id, x.UserId, x.Action, x.Entity, x.OldValue, x.NewValue, x.IpAddress, x.TraceId, x.CreatedDate, "Tariff"))
            .ToListAsync(ct);

        return shipmentLogs.Concat(tariffLogs)
            .OrderByDescending(x => x.CreatedDate)
            .Take(take)
            .ToList();
    }
}

public sealed record AuditLogDto(
    Guid Id,
    string UserId,
    string Action,
    string Entity,
    string? OldValue,
    string? NewValue,
    string? IpAddress,
    string? TraceId,
    DateTime CreatedDate,
    string Source);
