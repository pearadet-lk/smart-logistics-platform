using System.Text.Json;
using SmartLogistics.Shipment.Api.Services;

namespace SmartLogistics.Shipment.Api.Services;

public sealed class OutboxPublisherService(
    IServiceScopeFactory scopeFactory,
    KafkaEventPublisher kafka,
    ILogger<OutboxPublisherService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = scopeFactory.CreateScope();
            var shipments = scope.ServiceProvider.GetRequiredService<ShipmentService>();

            foreach (var message in await shipments.GetPendingOutboxAsync(stoppingToken))
            {
                try
                {
                    var payload = JsonSerializer.Deserialize<object>(message.Payload)!;
                    await kafka.PublishAsync(message.Topic, payload, stoppingToken);
                    await shipments.MarkOutboxPublishedAsync(message.Id, stoppingToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Outbox publish failed for {OutboxId}", message.Id);
                    await shipments.MarkOutboxFailedAsync(message.Id, stoppingToken);
                }
            }

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }
}
