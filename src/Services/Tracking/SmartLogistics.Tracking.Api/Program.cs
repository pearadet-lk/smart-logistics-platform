using Microsoft.EntityFrameworkCore;
using SmartLogistics.Shared.Extensions;
using SmartLogistics.Shared.Middleware;
using SmartLogistics.Tracking.Api.Consumers;
using SmartLogistics.Tracking.Api.Data;
using SmartLogistics.Tracking.Api.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddSmartLogisticsKeyVault(builder.Environment);
builder.AddSmartLogisticsSerilog();

builder.Services.AddControllers();
builder.Services.AddSmartLogisticsAuth(builder.Configuration);
builder.Services.AddSmartLogisticsHealthChecks();
builder.Services.AddSmartLogisticsOpenTelemetry(builder.Configuration, "tracking-api");
builder.Services.AddHostedService<ShipmentCreatedTrackingConsumer>();

builder.Services.AddDbContext<TrackingDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("TrackingDB")));

builder.Services.AddScoped<TrackingService>();

var app = builder.Build();

await app.MigrateSmartLogisticsDatabaseAsync<TrackingDbContext>(TrackingDbSeeder.SeedAsync);

app.UseAuthentication();
app.UseAuthorization();
app.UseTenantResolution();
app.MapControllers();
app.MapSmartLogisticsHealthChecks();

app.Run();
