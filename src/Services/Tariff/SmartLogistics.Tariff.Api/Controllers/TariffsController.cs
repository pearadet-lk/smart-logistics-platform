using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartLogistics.Shared.Audit;
using SmartLogistics.Shared.Authorization;
using SmartLogistics.Shared.Middleware;
using SmartLogistics.Shared.Tariffs;
using SmartLogistics.Tariff.Api.Services;

namespace SmartLogistics.Tariff.Api.Controllers;

[ApiController]
[Route("api/tariffs")]
[Authorize]
public class TariffsController(TariffService service) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = Permissions.TariffRead)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var tenant = HttpContext.Items[TenantMiddleware.TenantItemKey] as string;
        return Ok(await service.GetAllAsync(tenant, ct));
    }

    [HttpGet("versions/{id:guid}")]
    [Authorize(Policy = Permissions.TariffRead)]
    public async Task<IActionResult> GetVersion(Guid id, CancellationToken ct)
    {
        var tenant = HttpContext.Items[TenantMiddleware.TenantItemKey] as string;
        var version = (await service.GetAllAsync(tenant, ct)).FirstOrDefault(v => v.Id == id);
        return version is null ? NotFound() : Ok(version);
    }

    [HttpPost("versions")]
    [Authorize(Policy = Permissions.TariffUpdate)]
    public async Task<IActionResult> CreateVersion([FromBody] CreateTariffVersionRequest request, CancellationToken ct)
    {
        var user = User.FindFirstValue("preferred_username") ?? "system";
        var tenant = HttpContext.Items[TenantMiddleware.TenantItemKey] as string ?? request.TenantId ?? "DEFAULT";
        var (_, ip, traceId) = HttpContext.GetAuditContext();
        var version = await service.CreateDraftAsync(user, tenant, request.ContentJson, ip, traceId, ct);
        return Accepted(version);
    }

    [HttpPost("versions/{id:guid}/submit")]
    [Authorize(Policy = Permissions.TariffUpdate)]
    public async Task<IActionResult> Submit(Guid id, CancellationToken ct)
    {
        var (user, ip, traceId) = HttpContext.GetAuditContext();
        return await service.AdvanceWorkflowAsync(id, TariffWorkflowStatus.Submitted, user, ip, traceId, ct) is { } v
            ? Ok(v) : NotFound();
    }

    [HttpPost("versions/{id:guid}/approve")]
    [Authorize(Policy = Permissions.TariffApprove)]
    public async Task<IActionResult> Approve(Guid id, CancellationToken ct)
    {
        var (user, ip, traceId) = HttpContext.GetAuditContext();
        return await service.AdvanceWorkflowAsync(id, TariffWorkflowStatus.Approved, user, ip, traceId, ct) is { } v
            ? Ok(v) : NotFound();
    }

    [HttpPost("versions/{id:guid}/publish")]
    [Authorize(Policy = Permissions.TariffApprove)]
    public async Task<IActionResult> Publish(Guid id, CancellationToken ct)
    {
        var (user, ip, traceId) = HttpContext.GetAuditContext();
        return await service.AdvanceWorkflowAsync(id, TariffWorkflowStatus.Published, user, ip, traceId, ct) is { } v
            ? Ok(v) : NotFound();
    }

    [HttpGet("versions/compare")]
    [Authorize(Policy = Permissions.TariffRead)]
    public async Task<IActionResult> Compare([FromQuery] Guid v1, [FromQuery] Guid v2, CancellationToken ct)
    {
        var result = await service.CompareAsync(v1, v2, ct);
        return result is null ? NotFound() : Ok(result);
    }
}

public sealed record CreateTariffVersionRequest(string ContentJson, string? TenantId = null);

[ApiController]
[Route("api/tariffs/local-charges")]
[Authorize(Policy = Permissions.TariffRead)]
public class LocalChargesController : ControllerBase
{
    [HttpGet]
    public IActionResult GetAll() =>
        Ok(new { message = "Local charges placeholder", items = Array.Empty<object>() });
}

[ApiController]
[Route("api/tariffs/currency-rates")]
[Authorize(Policy = Permissions.TariffRead)]
public class CurrencyRatesController(TariffService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct) =>
        Ok(await service.GetExchangeRatesAsync(ct));

    [HttpGet("convert")]
    public async Task<IActionResult> Convert([FromQuery] decimal amount, [FromQuery] string from = "USD", [FromQuery] string to = "THB", CancellationToken ct = default)
    {
        var rates = await service.GetExchangeRatesAsync(ct);
        var rate = rates.FirstOrDefault(r => r.BaseCurrency == from && r.QuoteCurrency == to)?.Rate ?? 1m;
        return Ok(new { amount, from, to, rate, converted = amount * rate });
    }
}

[ApiController]
[Route("api/tariffs/lookups")]
[Authorize(Policy = Permissions.TariffRead)]
public class LookupsController(LookupCacheService lookups) : ControllerBase
{
    [HttpGet("ports")]
    public async Task<IActionResult> Ports(CancellationToken ct) =>
        Ok(await lookups.GetPortsAsync(ct));

    [HttpGet("countries")]
    public async Task<IActionResult> Countries(CancellationToken ct) =>
        Ok(await lookups.GetCountriesAsync(ct));
}

[ApiController]
[Route("api/tariffs/upload")]
[Authorize(Policy = Permissions.TariffUpdate)]
public class TariffUploadController(TariffService service) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Upload(IFormFile file, CancellationToken ct)
    {
        if (file.Length == 0) return BadRequest("Empty file");

        using var reader = new StreamReader(file.OpenReadStream());
        var content = await reader.ReadToEndAsync(ct);
        var user = User.FindFirstValue("preferred_username") ?? "system";
        var tenant = HttpContext.Items[TenantMiddleware.TenantItemKey] as string ?? "DEFAULT";
        var (_, ip, traceId) = HttpContext.GetAuditContext();

        var version = await service.CreateDraftAsync(user, tenant,
            $$"""{"fileName":"{{file.FileName}}","preview":{{JsonSerializer.Serialize(content[..Math.Min(content.Length, 500)])}}}""",
            ip, traceId, ct);

        return Accepted(new { message = "Tariff uploaded and validated", version });
    }
}
