using System.Text.Json;
using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using SmartLogistics.Tracking.Api.Data;
using SmartLogistics.Tracking.Api.Services;

namespace SmartLogistics.Tracking.Api.Consumers;

public sealed class ShipmentCreatedTrackingConsumer(
    IConfiguration configuration,
    IServiceScopeFactory scopeFactory,
    ILogger<ShipmentCreatedTrackingConsumer> logger) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken) =>
        Task.Run(() => ConsumeLoop(stoppingToken), stoppingToken);

    private async void ConsumeLoop(CancellationToken stoppingToken)
    {
        var bootstrap = configuration["Kafka:BootstrapServers"];
        var topic = configuration["Kafka:ShipmentCreatedTopic"] ?? "shipment.created";

        if (string.IsNullOrWhiteSpace(bootstrap))
        {
            logger.LogWarning("Kafka not configured — tracking consumer disabled");
            return;
        }

        using var consumer = new ConsumerBuilder<string, string>(new ConsumerConfig
        {
            BootstrapServers = bootstrap,
            GroupId = "tracking-service",
            AutoOffsetReset = AutoOffsetReset.Earliest
        }).Build();

        consumer.Subscribe(topic);
        logger.LogInformation("Tracking service subscribed to {Topic}", topic);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var result = consumer.Consume(stoppingToken);
                using var scope = scopeFactory.CreateScope();
                var tracking = scope.ServiceProvider.GetRequiredService<TrackingService>();

                var payload = JsonDocument.Parse(result.Message.Value);
                var shipmentId = payload.RootElement.GetProperty("ShipmentId").GetGuid();
                var origin = payload.RootElement.TryGetProperty("OriginPort", out var port)
                    ? port.GetString() ?? "Unknown"
                    : "Unknown";

                await tracking.RecordBookedAsync(shipmentId, origin, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Tracking consumer error");
            }
        }
    }
}
