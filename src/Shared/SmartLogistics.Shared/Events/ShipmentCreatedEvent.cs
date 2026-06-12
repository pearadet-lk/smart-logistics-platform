namespace SmartLogistics.Shared.Events;

/// <summary>
/// Published to Kafka when a shipment is created.
/// Consumers: Billing, Notification, Reporting (placeholder).
/// </summary>
public sealed record ShipmentCreatedEvent(
    Guid ShipmentId,
    string CustomerId,
    string PartnerId,
    DateTime CreatedAt);
