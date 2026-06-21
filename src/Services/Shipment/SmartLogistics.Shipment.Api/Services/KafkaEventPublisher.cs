using Confluent.Kafka;
using System.Text.Json;

namespace SmartLogistics.Shipment.Api.Services;

public sealed class KafkaEventPublisher : IDisposable
{
    private readonly IProducer<string, string>? _producer;
    private readonly ILogger<KafkaEventPublisher> _logger;
    private readonly bool _enabled;

    public KafkaEventPublisher(IConfiguration configuration, ILogger<KafkaEventPublisher> logger)
    {
        _logger = logger;
        var bootstrap = configuration["Kafka:BootstrapServers"];
        _enabled = !string.IsNullOrWhiteSpace(bootstrap);

        if (_enabled)
        {
            _producer = new ProducerBuilder<string, string>(new ProducerConfig
            {
                BootstrapServers = bootstrap,
                Acks = Acks.Leader
            }).Build();
        }
    }

    public async Task PublishAsync(string topic, object payload, CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(payload);

        if (!_enabled || _producer is null)
        {
            _logger.LogWarning("Kafka disabled — event not published to {Topic}: {Payload}", topic, json);
            return;
        }

        try
        {
            await _producer.ProduceAsync(topic, new Message<string, string>
            {
                Key = Guid.NewGuid().ToString(),
                Value = json
            }, cancellationToken);
            _logger.LogInformation("Published event to {Topic}", topic);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish to {Topic}", topic);
            throw;
        }
    }

    public void Dispose() => _producer?.Dispose();
}
