var builder = WebApplication.CreateBuilder(args);

// TODO: EF Core + PostgreSQL (ShipmentDB), Kafka producer, JWT validation, OpenTelemetry
builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "shipment-api", database = "ShipmentDB" }));

app.Run();
