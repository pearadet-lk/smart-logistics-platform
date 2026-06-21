using Microsoft.EntityFrameworkCore;
using SmartLogistics.Reporting.Api.Data;
using SmartLogistics.Reporting.Api.Services;
using SmartLogistics.Shared.Extensions;
using SmartLogistics.Shared.Middleware;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddSmartLogisticsKeyVault(builder.Environment);
builder.AddSmartLogisticsSerilog();

builder.Services.AddControllers();
builder.Services.AddSmartLogisticsAuth(builder.Configuration);
builder.Services.AddSmartLogisticsHealthChecks();
builder.Services.AddSmartLogisticsOpenTelemetry(builder.Configuration, "reporting-api");

builder.Services.AddDbContext<ShipmentReadContext>(o =>
    o.UseNpgsql(builder.Configuration.GetConnectionString("ShipmentDB")));
builder.Services.AddDbContext<TariffReadContext>(o =>
    o.UseNpgsql(builder.Configuration.GetConnectionString("TariffDB")));
builder.Services.AddDbContext<BillingReadContext>(o =>
    o.UseNpgsql(builder.Configuration.GetConnectionString("BillingDB")));

builder.Services.AddScoped<AnalyticsService>();
builder.Services.AddScoped<AuditQueryService>();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
app.UseTenantResolution();
app.MapControllers();
app.MapSmartLogisticsHealthChecks();

app.Run();
