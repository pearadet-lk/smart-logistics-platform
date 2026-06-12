using Microsoft.AspNetCore.Mvc;

namespace SmartLogistics.Notification.Api.Controllers;

[ApiController]
[Route("api/notifications")]
public class NotificationsController : ControllerBase
{
    /// <summary>Placeholder — consumes Kafka events and sends notifications.</summary>
    [HttpGet]
    public IActionResult GetAll() =>
        Ok(new { message = "Placeholder — notification history", items = Array.Empty<object>() });

    [HttpPost("send")]
    public IActionResult Send([FromBody] object request) =>
        Accepted(new { message = "Placeholder — notification queued", id = Guid.NewGuid() });
}
