using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartLogistics.Reporting.Api.Services;
using SmartLogistics.Shared.Authorization;
using SmartLogistics.Shared.Middleware;

namespace SmartLogistics.Reporting.Api.Controllers;

[ApiController]
[Route("api/analytics")]
[Authorize]
public class AnalyticsController(AnalyticsService analytics) : ControllerBase
{
    [HttpGet("dashboard")]
    [Authorize(Policy = Permissions.ShipmentRead)]
    public async Task<IActionResult> Dashboard(CancellationToken ct)
    {
        var tenant = HttpContext.Items[TenantMiddleware.TenantItemKey] as string;
        return Ok(await analytics.GetDashboardAsync(tenant, ct));
    }
}

[ApiController]
[Route("api/audit")]
[Authorize(Policy = "RequireAdmin")]
public class AuditController(AuditQueryService audit) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int take = 100, CancellationToken ct = default) =>
        Ok(await audit.GetAllAsync(take, ct));
}
