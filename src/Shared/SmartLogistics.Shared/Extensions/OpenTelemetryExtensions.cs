using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace SmartLogistics.Shared.Extensions;

public static class OpenTelemetryExtensions
{
    public static IServiceCollection AddSmartLogisticsOpenTelemetry(
        this IServiceCollection services,
        IConfiguration configuration,
        string serviceName)
    {
        var jaegerEndpoint = configuration["OpenTelemetry:JaegerEndpoint"];

        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(serviceName))
            .WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddConsoleExporter();

                if (!string.IsNullOrWhiteSpace(jaegerEndpoint))
                {
                    tracing.AddOtlpExporter(options => options.Endpoint = new Uri(jaegerEndpoint));
                }
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddConsoleExporter();

                if (!string.IsNullOrWhiteSpace(jaegerEndpoint))
                {
                    metrics.AddOtlpExporter(options => options.Endpoint = new Uri(jaegerEndpoint));
                }
            });

        return services;
    }
}
