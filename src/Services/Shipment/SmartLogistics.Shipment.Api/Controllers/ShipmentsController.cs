using Microsoft.AspNetCore.Mvc;

namespace SmartLogistics.Shipment.Api.Controllers;

[ApiController]
[Route("api/shipments")]
public class ShipmentsController : ControllerBase
{
    /// <summary>Placeholder — Booking, Shipment, Container, Tracking.</summary>
    [HttpGet]
    public IActionResult GetAll() =>
        Ok(new { message = "Placeholder — list shipments", items = Array.Empty<object>() });

    [HttpGet("{id:guid}")]
    public IActionResult GetById(Guid id) =>
        Ok(new { message = "Placeholder — get shipment", id });

    [HttpPost]
    public IActionResult Create([FromBody] object request) =>
        Accepted(new { message = "Placeholder — shipment created, event published to Kafka", id = Guid.NewGuid() });

    [HttpPut("{id:guid}")]
    public IActionResult Update(Guid id, [FromBody] object request) =>
        Ok(new { message = "Placeholder — update shipment", id });

    [HttpDelete("{id:guid}")]
    public IActionResult Delete(Guid id) =>
        NoContent();
}

[ApiController]
[Route("api/shipments/{shipmentId:guid}/tracking")]
public class TrackingController : ControllerBase
{
    [HttpGet]
    public IActionResult GetTracking(Guid shipmentId) =>
        Ok(new { message = "Placeholder — tracking events", shipmentId, events = Array.Empty<object>() });
}
