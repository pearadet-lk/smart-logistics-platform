namespace SmartLogistics.Shared.Shipments;

public sealed class ContainerTracking
{
    public Guid Id { get; set; }
    public Guid ShipmentId { get; set; }
    public string ContainerNo { get; set; } = string.Empty;
    public string SealNo { get; set; } = string.Empty;
    public string CurrentPort { get; set; } = string.Empty;
    public DateTime? Etd { get; set; }
    public DateTime? Eta { get; set; }
}
