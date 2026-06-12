var builder = WebApplication.CreateBuilder(args);

// TODO: EF Core + PostgreSQL (TariffDB), JWT validation, OpenTelemetry
builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "tariff-api", database = "TariffDB" }));

app.Run();
