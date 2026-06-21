namespace SmartLogistics.Shared.Billing;

public sealed class InvoiceDraftDto
{
    public Guid Id { get; set; }
    public Guid ShipmentId { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
}
