using Microsoft.AspNetCore.Mvc;

namespace SmartLogistics.Billing.Api.Controllers;

[ApiController]
[Route("api/invoices")]
public class InvoicesController : ControllerBase
{
    /// <summary>Placeholder — Invoice, Payment, Tax.</summary>
    [HttpGet]
    public IActionResult GetAll() =>
        Ok(new { message = "Placeholder — list invoices", items = Array.Empty<object>() });

    [HttpGet("{id:guid}")]
    public IActionResult GetById(Guid id) =>
        Ok(new { message = "Placeholder — get invoice", id });

    [HttpPost]
    public IActionResult Create([FromBody] object request) =>
        Accepted(new { message = "Placeholder — invoice created", id = Guid.NewGuid() });

    [HttpPost("{id:guid}/payments")]
    public IActionResult RecordPayment(Guid id, [FromBody] object request) =>
        Ok(new { message = "Placeholder — payment recorded", invoiceId = id });
}
