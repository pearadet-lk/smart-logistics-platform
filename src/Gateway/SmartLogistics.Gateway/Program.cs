using System.Threading.RateLimiting;
using SmartLogistics.Shared.Extensions;
using SmartLogistics.Shared.Features;

var builder = WebApplication.CreateBuilder(args);
builder.AddSmartLogisticsSerilog();

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddSmartLogisticsAuth(builder.Configuration);

builder.Services.Configure<FeatureFlags>(builder.Configuration.GetSection("FeatureFlags"));

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("customer", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.User.Identity?.Name ?? httpContext.Connection.RemoteIpAddress?.ToString() ?? "anon",
            _ => new FixedWindowRateLimiterOptions { PermitLimit = 100, Window = TimeSpan.FromMinutes(1) }));
    options.AddPolicy("partner", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.User.Identity?.Name ?? "partner",
            _ => new FixedWindowRateLimiterOptions { PermitLimit = 1000, Window = TimeSpan.FromMinutes(1) }));
});

builder.Services.AddSmartLogisticsHealthChecks();
builder.Services.AddSmartLogisticsOpenTelemetry(builder.Configuration, "gateway");

var app = builder.Build();

app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.Use(async (context, next) =>
{
    var correlationId = context.Request.Headers["X-Correlation-Id"].FirstOrDefault()
        ?? Guid.NewGuid().ToString();
    context.Response.Headers["X-Correlation-Id"] = correlationId;
    context.Items["CorrelationId"] = correlationId;
    await next();
});

app.MapSmartLogisticsHealthChecks();
app.MapGet("/api/feature-flags", (Microsoft.Extensions.Options.IOptions<FeatureFlags> flags) =>
    Results.Ok(flags.Value));

app.MapReverseProxy().RequireRateLimiting("customer");

app.Run();
