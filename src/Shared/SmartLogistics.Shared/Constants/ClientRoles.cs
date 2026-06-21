namespace SmartLogistics.Shared.Constants;

/// <summary>Keycloak client roles per API resource (fine-grained permissions).</summary>
public static class ShipmentClientRoles
{
    public const string Read = "Shipment.Read";
    public const string Create = "Shipment.Create";
    public const string Update = "Shipment.Update";
    public const string Delete = "Shipment.Delete";
}

public static class TariffClientRoles
{
    public const string Read = "Tariff.Read";
    public const string Update = "Tariff.Update";
    public const string Approve = "Tariff.Approve";
}

public static class BillingClientRoles
{
    public const string Read = "Invoice.Read";
    public const string Write = "Invoice.Write";
}
