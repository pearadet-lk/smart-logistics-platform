using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartLogistics.Ai.Api.Services;
using SmartLogistics.Shared.Middleware;

namespace SmartLogistics.Ai.Api.Controllers;

[ApiController]
[Route("api/ai")]
[Authorize]
public class AssistantController(LogisticsAssistantService assistant) : ControllerBase
{
    [HttpPost("chat")]
    public async Task<IActionResult> Chat([FromBody] ChatRequest request, CancellationToken ct)
    {
        var tenant = HttpContext.Items[TenantMiddleware.TenantItemKey] as string;
        var answer = await assistant.AskAsync(request.Question, tenant, ct);
        return Ok(new { question = request.Question, answer, tenant });
    }
}

public sealed record ChatRequest(string Question);
