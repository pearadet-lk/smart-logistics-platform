namespace SmartLogistics.Shared.Shipments;

public sealed class ShipmentStatusHistory
{
    public Guid Id { get; set; }
    public Guid ShipmentId { get; set; }
    public ShipmentStatus Status { get; set; }
    public string Location { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
}
