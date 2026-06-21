using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartLogistics.Shared.Audit;
using SmartLogistics.Shared.Authorization;
using SmartLogistics.Shared.Workflow;
using SmartLogistics.Tariff.Api.Services;

namespace SmartLogistics.Tariff.Api.Controllers;

[ApiController]
[Route("api/tariffs/workflow")]
[Authorize]
public class WorkflowController(WorkflowService workflow) : ControllerBase
{
    [HttpGet("{tariffVersionId:guid}")]
    [Authorize(Policy = Permissions.TariffRead)]
    public async Task<IActionResult> Get(Guid tariffVersionId, CancellationToken ct)
    {
        var instance = await workflow.GetForEntityAsync("TariffVersion", tariffVersionId, ct);
        if (instance is null) return NotFound();

        var history = await workflow.GetHistoryAsync(instance.Id, ct);
        return Ok(new { instance, history });
    }

    [HttpPost("{tariffVersionId:guid}/approve")]
    [Authorize(Policy = Permissions.TariffApprove)]
    public async Task<IActionResult> Approve(Guid tariffVersionId, [FromBody] WorkflowActionRequest? request, CancellationToken ct)
    {
        var actor = User.Identity?.Name ?? "system";
        var (_, ip, traceId) = HttpContext.GetAuditContext();
        var (instance, version) = await workflow.AdvanceAsync(
            tariffVersionId, WorkflowStatus.Approved, actor, request?.Comment, ip, traceId, ct);
        return instance is null ? NotFound() : Ok(new { instance, version });
    }

    [HttpPost("{tariffVersionId:guid}/reject")]
    [Authorize(Policy = Permissions.TariffApprove)]
    public async Task<IActionResult> Reject(Guid tariffVersionId, [FromBody] WorkflowActionRequest request, CancellationToken ct)
    {
        var actor = User.Identity?.Name ?? "system";
        var (_, ip, traceId) = HttpContext.GetAuditContext();
        var (instance, version) = await workflow.AdvanceAsync(
            tariffVersionId, WorkflowStatus.Rejected, actor, request.Comment, ip, traceId, ct);
        return instance is null ? NotFound() : Ok(new { instance, version });
    }

    [HttpPost("{tariffVersionId:guid}/publish")]
    [Authorize(Policy = Permissions.TariffApprove)]
    public async Task<IActionResult> Publish(Guid tariffVersionId, CancellationToken ct)
    {
        var actor = User.Identity?.Name ?? "system";
        var (_, ip, traceId) = HttpContext.GetAuditContext();
        var (instance, version) = await workflow.AdvanceAsync(
            tariffVersionId, WorkflowStatus.Published, actor, null, ip, traceId, ct);
        return instance is null ? NotFound() : Ok(new { instance, version });
    }
}

public sealed record WorkflowActionRequest(string? Comment);
