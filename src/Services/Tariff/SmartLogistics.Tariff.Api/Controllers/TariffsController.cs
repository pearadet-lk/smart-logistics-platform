using Microsoft.AspNetCore.Mvc;

namespace SmartLogistics.Tariff.Api.Controllers;

[ApiController]
[Route("api/tariffs")]
public class TariffsController : ControllerBase
{
    /// <summary>Placeholder — Tariffs, Local Charges, Currency Rates.</summary>
    [HttpGet]
    public IActionResult GetAll() =>
        Ok(new { message = "Placeholder — list tariffs", items = Array.Empty<object>() });

    [HttpGet("{id:guid}")]
    public IActionResult GetById(Guid id) =>
        Ok(new { message = "Placeholder — get tariff", id });

    [HttpPost]
    public IActionResult Create([FromBody] object request) =>
        Accepted(new { message = "Placeholder — tariff created", id = Guid.NewGuid() });
}

[ApiController]
[Route("api/tariffs/local-charges")]
public class LocalChargesController : ControllerBase
{
    [HttpGet]
    public IActionResult GetAll() =>
        Ok(new { message = "Placeholder — local charges", items = Array.Empty<object>() });
}

[ApiController]
[Route("api/tariffs/currency-rates")]
public class CurrencyRatesController : ControllerBase
{
    [HttpGet]
    public IActionResult GetAll() =>
        Ok(new { message = "Placeholder — currency rates", items = Array.Empty<object>() });
}
