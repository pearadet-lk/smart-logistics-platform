namespace SmartLogistics.Shared.Constants;

/// <summary>Keycloak client roles per API resource.</summary>
public static class ShipmentClientRoles
{
    public const string Read = "Shipment.Read";
    public const string Write = "Shipment.Write";
    public const string Delete = "Shipment.Delete";
}

public static class BillingClientRoles
{
    public const string Read = "Invoice.Read";
    public const string Write = "Invoice.Write";
}
