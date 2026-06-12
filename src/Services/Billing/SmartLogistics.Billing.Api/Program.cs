var builder = WebApplication.CreateBuilder(args);

// TODO: EF Core + PostgreSQL (BillingDB), Kafka consumer, Client Credentials for S2S calls, OpenTelemetry
builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "billing-api", database = "BillingDB" }));

app.Run();
