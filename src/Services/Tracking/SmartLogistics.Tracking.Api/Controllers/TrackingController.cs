using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartLogistics.Shared.Authorization;
using SmartLogistics.Tracking.Api.Services;

namespace SmartLogistics.Tracking.Api.Controllers;

[ApiController]
[Route("api/tracking/shipments/{shipmentId:guid}")]
[Authorize(Policy = Permissions.ShipmentRead)]
public class TrackingController(TrackingService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(Guid shipmentId, CancellationToken ct)
    {
        var timeline = await service.GetTimelineAsync(shipmentId, ct);
        var containers = await service.GetContainersAsync(shipmentId, ct);
        return Ok(new { shipmentId, timeline, containers });
    }
}
