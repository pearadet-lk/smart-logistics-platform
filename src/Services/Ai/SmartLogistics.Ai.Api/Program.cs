using SmartLogistics.Ai.Api.Services;
using SmartLogistics.Shared.Extensions;
using SmartLogistics.Shared.Middleware;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddSmartLogisticsKeyVault(builder.Environment);
builder.AddSmartLogisticsSerilog();

builder.Services.AddControllers();
builder.Services.AddSmartLogisticsAuth(builder.Configuration);
builder.Services.AddSmartLogisticsHealthChecks();
builder.Services.AddSmartLogisticsOpenTelemetry(builder.Configuration, "ai-api");
builder.Services.AddHttpClient("azure-openai");
builder.Services.AddSingleton<LogisticsAssistantService>();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
app.UseTenantResolution();
app.MapControllers();
app.MapSmartLogisticsHealthChecks();

app.Run();
