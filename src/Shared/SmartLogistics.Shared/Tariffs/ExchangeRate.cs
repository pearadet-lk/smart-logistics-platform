namespace SmartLogistics.Shared.Tariffs;

public sealed class ExchangeRate
{
    public Guid Id { get; set; }
    public string BaseCurrency { get; set; } = "USD";
    public string QuoteCurrency { get; set; } = string.Empty;
    public decimal Rate { get; set; }
    public DateTime EffectiveDate { get; set; }
    public DateTime ImportedAt { get; set; }
}
