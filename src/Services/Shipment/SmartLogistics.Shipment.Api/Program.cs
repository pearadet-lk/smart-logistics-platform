using Microsoft.EntityFrameworkCore;
using SmartLogistics.Shared.Auth;
using SmartLogistics.Shared.Extensions;
using SmartLogistics.Shared.Middleware;
using SmartLogistics.Shipment.Api.Data;
using SmartLogistics.Shipment.Api.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddSmartLogisticsKeyVault(builder.Environment);
builder.AddSmartLogisticsSerilog();

builder.Services.AddControllers();
builder.Services.AddSmartLogisticsAuth(builder.Configuration);
builder.Services.AddSmartLogisticsServiceAuth();
builder.Services.AddSmartLogisticsHealthChecks();
builder.Services.AddSmartLogisticsOpenTelemetry(builder.Configuration, "shipment-api");
builder.Services.AddSmartLogisticsRedisCache(builder.Configuration);

builder.Services.AddDbContext<ShipmentDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("ShipmentDB")));

builder.Services.AddScoped<ShipmentService>();
builder.Services.AddSingleton<KafkaEventPublisher>();
builder.Services.AddHostedService<OutboxPublisherService>();

var app = builder.Build();

await app.MigrateSmartLogisticsDatabaseAsync<ShipmentDbContext>(ShipmentDbSeeder.SeedAsync);

app.UseAuthentication();
app.UseAuthorization();
app.UseTenantResolution();
app.MapControllers();
app.MapSmartLogisticsHealthChecks();

app.Run();
