namespace SmartLogistics.Shared.Events;

public sealed record ShipmentCreatedEvent(
    Guid ShipmentId,
    string CustomerId,
    string PartnerId,
    DateTime CreatedAt,
    string OriginPort);
