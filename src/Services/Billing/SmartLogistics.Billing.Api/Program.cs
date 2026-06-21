using Microsoft.EntityFrameworkCore;
using SmartLogistics.Billing.Api.Data;
using SmartLogistics.Billing.Api.Services;
using SmartLogistics.Shared.Auth;
using SmartLogistics.Shared.Extensions;
using SmartLogistics.Shared.Middleware;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddSmartLogisticsKeyVault(builder.Environment);
builder.AddSmartLogisticsSerilog();

builder.Services.AddControllers();
builder.Services.AddSmartLogisticsAuth(builder.Configuration);
builder.Services.AddSmartLogisticsServiceAuth();
builder.Services.AddSmartLogisticsHealthChecks();
builder.Services.AddSmartLogisticsOpenTelemetry(builder.Configuration, "billing-api");
builder.Services.AddHostedService<ShipmentCreatedConsumer>();

builder.Services.AddDbContext<BillingDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("BillingDB")));

var app = builder.Build();

await app.MigrateSmartLogisticsDatabaseAsync<BillingDbContext>(BillingDbSeeder.SeedAsync);

app.UseAuthentication();
app.UseAuthorization();
app.UseTenantResolution();
app.MapControllers();
app.MapSmartLogisticsHealthChecks();

app.Run();
