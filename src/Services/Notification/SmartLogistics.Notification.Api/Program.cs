var builder = WebApplication.CreateBuilder(args);

// TODO: Kafka consumer (shipment.created), email/SMS providers, OpenTelemetry
builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "notification-api" }));

app.Run();
