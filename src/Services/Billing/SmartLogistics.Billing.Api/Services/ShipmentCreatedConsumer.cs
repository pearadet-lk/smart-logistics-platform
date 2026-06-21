using System.Text.Json;
using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using SmartLogistics.Billing.Api.Data;
using SmartLogistics.Shared.Auth;

namespace SmartLogistics.Billing.Api.Services;

public sealed class ShipmentCreatedConsumer(
    IConfiguration configuration,
    IServiceScopeFactory scopeFactory,
    ClientCredentialsTokenProvider tokenProvider,
    ILogger<ShipmentCreatedConsumer> logger) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken) =>
        Task.Run(() => ConsumeLoop(stoppingToken), stoppingToken);

    private async void ConsumeLoop(CancellationToken stoppingToken)
    {
        var bootstrap = configuration["Kafka:BootstrapServers"];
        var topic = configuration["Kafka:ShipmentCreatedTopic"] ?? "shipment.created";

        if (string.IsNullOrWhiteSpace(bootstrap))
        {
            logger.LogWarning("Kafka not configured — billing consumer disabled");
            return;
        }

        _ = await tokenProvider.GetTokenAsync(stoppingToken);

        using var consumer = new ConsumerBuilder<string, string>(new ConsumerConfig
        {
            BootstrapServers = bootstrap,
            GroupId = "billing-service",
            AutoOffsetReset = AutoOffsetReset.Earliest
        }).Build();

        consumer.Subscribe(topic);
        logger.LogInformation("Billing subscribed to {Topic}", topic);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var result = consumer.Consume(stoppingToken);
                using var scope = scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<BillingDbContext>();

                var eventId = result.Message.Key ?? result.Message.Value;
                if (await db.ProcessedEvents.AnyAsync(e => e.EventId == eventId, stoppingToken))
                {
                    continue;
                }

                var payload = JsonDocument.Parse(result.Message.Value);
                var shipmentId = payload.RootElement.GetProperty("ShipmentId").GetGuid();
                var customerId = payload.RootElement.GetProperty("CustomerId").GetString() ?? "unknown";
                var partnerId = payload.RootElement.TryGetProperty("PartnerId", out var partner)
                    ? partner.GetString() ?? "DEFAULT"
                    : "DEFAULT";

                db.InvoiceDrafts.Add(new InvoiceDraft
                {
                    Id = Guid.NewGuid(),
                    ShipmentId = shipmentId,
                    CustomerId = customerId,
                    TenantId = partnerId,
                    Amount = 1200m,
                    Status = "Draft",
                    CreatedDate = DateTime.UtcNow
                });

                db.ProcessedEvents.Add(new ProcessedEvent
                {
                    Id = Guid.NewGuid(),
                    EventId = eventId,
                    Topic = topic,
                    ProcessedDate = DateTime.UtcNow
                });

                await db.SaveChangesAsync(stoppingToken);
                logger.LogInformation("Draft invoice created for shipment {ShipmentId}", shipmentId);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Billing consumer error");
            }
        }
    }
}
