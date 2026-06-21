namespace SmartLogistics.Shared.Shipments;

public sealed class Shipment
{
    public Guid Id { get; set; }
    public string TenantId { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    public string OriginPort { get; set; } = string.Empty;
    public string DestinationPort { get; set; } = string.Empty;
    public ShipmentStatus Status { get; set; }
    public DateTime CreatedDate { get; set; }
}
