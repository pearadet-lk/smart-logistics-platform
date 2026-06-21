using System.Text.Json;
using Confluent.Kafka;
using SmartLogistics.Notification.Api.Hubs;
using SmartLogistics.Notification.Api.Services;

namespace SmartLogistics.Notification.Api.Services;

public sealed class ShipmentCreatedConsumer(
    IConfiguration configuration,
    NotificationDispatcher dispatcher,
    ILogger<ShipmentCreatedConsumer> logger) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken) =>
        Task.Run(() => ConsumeLoop(stoppingToken), stoppingToken);

    private void ConsumeLoop(CancellationToken stoppingToken)
    {
        var bootstrap = configuration["Kafka:BootstrapServers"];
        var topic = configuration["Kafka:ShipmentCreatedTopic"] ?? "shipment-created";

        if (string.IsNullOrWhiteSpace(bootstrap))
        {
            logger.LogWarning("Kafka not configured — notification consumer disabled");
            return;
        }

        using var consumer = new ConsumerBuilder<string, string>(new ConsumerConfig
        {
            BootstrapServers = bootstrap,
            GroupId = "notification-service",
            AutoOffsetReset = AutoOffsetReset.Earliest
        }).Build();

        consumer.Subscribe(topic);
        logger.LogInformation("Subscribed to {Topic}", topic);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var result = consumer.Consume(stoppingToken);
                var payload = JsonDocument.Parse(result.Message.Value);
                var tenant = payload.RootElement.TryGetProperty("PartnerId", out var p)
                    ? p.GetString() ?? "DEFAULT"
                    : payload.RootElement.TryGetProperty("partnerId", out var p2)
                        ? p2.GetString() ?? "DEFAULT"
                        : "DEFAULT";

                var notification = new RealtimeNotification(
                    "ShipmentCreated",
                    "New Shipment",
                    "A shipment was created and queued for billing",
                    DateTime.UtcNow,
                    JsonSerializer.Deserialize<object>(result.Message.Value));

                dispatcher.SendToTenantAsync(tenant, notification).GetAwaiter().GetResult();
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Notification consumer error");
            }
        }
    }
}
