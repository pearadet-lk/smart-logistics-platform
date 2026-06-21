namespace SmartLogistics.Shared.Tariffs;

public enum TariffWorkflowStatus
{
    Draft,
    Submitted,
    Approved,
    Published
}

public sealed class TariffVersion
{
    public Guid Id { get; set; }
    public int VersionNo { get; set; }
    public DateTime EffectiveDate { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public TariffWorkflowStatus Status { get; set; }
    public string TenantId { get; set; } = string.Empty;
    public string ContentJson { get; set; } = "{}";
}
