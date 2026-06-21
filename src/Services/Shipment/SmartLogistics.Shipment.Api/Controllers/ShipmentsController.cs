using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartLogistics.Shared.Audit;
using SmartLogistics.Shared.Authorization;
using SmartLogistics.Shared.Events;
using SmartLogistics.Shared.Middleware;
using SmartLogistics.Shared.Shipments;
using ShipmentEntity = SmartLogistics.Shared.Shipments.Shipment;
using SmartLogistics.Shipment.Api.Services;

namespace SmartLogistics.Shipment.Api.Controllers;

[ApiController]
[Route("api/shipments")]
[Authorize]
public class ShipmentsController(ShipmentService service, IConfiguration configuration) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = Permissions.ShipmentRead)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var tenant = HttpContext.Items[TenantMiddleware.TenantItemKey] as string;
        return Ok(await service.GetAllAsync(tenant, ct));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = Permissions.ShipmentRead)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var tenant = HttpContext.Items[TenantMiddleware.TenantItemKey] as string;
        var shipment = await service.GetByIdAsync(id, tenant, ct);
        return shipment is null ? NotFound() : Ok(shipment);
    }

    [HttpPost]
    [Authorize(Policy = Permissions.ShipmentCreate)]
    public async Task<IActionResult> Create([FromBody] CreateShipmentRequest request, CancellationToken ct)
    {
        var tenant = HttpContext.Items[TenantMiddleware.TenantItemKey] as string ?? request.TenantId;
        var (userId, ip, traceId) = HttpContext.GetAuditContext();

        var shipment = await service.CreateAsync(new ShipmentEntity
        {
            TenantId = tenant ?? "DEFAULT",
            CustomerId = request.CustomerId,
            OriginPort = request.OriginPort,
            DestinationPort = request.DestinationPort
        }, userId, ip, traceId, ct);

        var evt = new ShipmentCreatedEvent(
            shipment.Id, shipment.CustomerId, tenant ?? "DEFAULT", DateTime.UtcNow, shipment.OriginPort);
        var topic = configuration["Kafka:ShipmentCreatedTopic"] ?? "shipment.created";
        var outbox = await service.EnqueueOutboxAsync(topic, JsonSerializer.Serialize(evt), ct);

        return Accepted(new { shipment, outboxMessageId = outbox.Id });
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = Permissions.ShipmentUpdate)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateShipmentRequest request, CancellationToken ct)
    {
        var tenant = HttpContext.Items[TenantMiddleware.TenantItemKey] as string;
        var (userId, ip, traceId) = HttpContext.GetAuditContext();
        var shipment = await service.UpdateAsync(id, tenant, userId, request.OriginPort, request.DestinationPort, ip, traceId, ct);
        return shipment is null ? NotFound() : Ok(shipment);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Permissions.ShipmentDelete)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var tenant = HttpContext.Items[TenantMiddleware.TenantItemKey] as string;
        var (userId, ip, traceId) = HttpContext.GetAuditContext();
        var deleted = await service.DeleteAsync(id, tenant, userId, ip, traceId, ct);
        return deleted ? NoContent() : NotFound();
    }
}

public sealed record CreateShipmentRequest(
    string CustomerId,
    string OriginPort,
    string DestinationPort,
    string? TenantId = null);

public sealed record UpdateShipmentRequest(string? OriginPort, string? DestinationPort);
