using Microsoft.AspNetCore.SignalR;
using SmartLogistics.Notification.Api.Hubs;
using SmartLogistics.Notification.Api.Services;
using SmartLogistics.Shared.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.AddSmartLogisticsSerilog();

builder.Services.AddControllers();
builder.Services.AddSmartLogisticsAuth(builder.Configuration);
builder.Services.AddSmartLogisticsHealthChecks();
builder.Services.AddSmartLogisticsOpenTelemetry(builder.Configuration, "notification-api");
builder.Services.AddSignalR();
builder.Services.AddSingleton<NotificationDispatcher>();
builder.Services.AddHostedService<ShipmentCreatedConsumer>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
});

var app = builder.Build();

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<NotificationHub>("/hubs/notifications");
app.MapSmartLogisticsHealthChecks();

app.Run();
