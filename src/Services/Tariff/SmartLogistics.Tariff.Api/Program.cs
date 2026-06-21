using Microsoft.EntityFrameworkCore;
using SmartLogistics.Shared.Extensions;
using SmartLogistics.Shared.Middleware;
using SmartLogistics.Tariff.Api.Data;
using SmartLogistics.Tariff.Api.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddSmartLogisticsKeyVault(builder.Environment);
builder.AddSmartLogisticsSerilog();

builder.Services.AddControllers();
builder.Services.AddSmartLogisticsAuth(builder.Configuration);
builder.Services.AddSmartLogisticsHealthChecks();
builder.Services.AddSmartLogisticsOpenTelemetry(builder.Configuration, "tariff-api");
builder.Services.AddSmartLogisticsRedisCache(builder.Configuration);

builder.Services.AddDbContext<TariffDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("TariffDB")));

builder.Services.AddScoped<TariffService>();
builder.Services.AddScoped<WorkflowService>();
builder.Services.AddSingleton<LookupCacheService>();
builder.Services.AddHostedService<ExchangeRateImportService>();

var app = builder.Build();

await app.MigrateSmartLogisticsDatabaseAsync<TariffDbContext>(TariffDbSeeder.SeedAsync);

app.UseAuthentication();
app.UseAuthorization();
app.UseTenantResolution();
app.MapControllers();
app.MapSmartLogisticsHealthChecks();

app.Run();
