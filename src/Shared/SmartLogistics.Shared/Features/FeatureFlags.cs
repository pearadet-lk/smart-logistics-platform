namespace SmartLogistics.Shared.Features;

public sealed class FeatureFlags
{
    public bool NewTariffScreen { get; set; } = true;
    public bool BillingModule { get; set; } = true;
    public bool AiAssistant { get; set; } = false;
    public bool PwaOfflineLookups { get; set; } = true;
}
