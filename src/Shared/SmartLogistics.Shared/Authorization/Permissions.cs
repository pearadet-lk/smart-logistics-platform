namespace SmartLogistics.Shared.Authorization;

/// <summary>Fine-grained permissions mapped from Keycloak client roles.</summary>
public static class Permissions
{
    public const string ShipmentRead = "Shipment.Read";
    public const string ShipmentCreate = "Shipment.Create";
    public const string ShipmentUpdate = "Shipment.Update";
    public const string ShipmentDelete = "Shipment.Delete";

    public const string TariffRead = "Tariff.Read";
    public const string TariffUpdate = "Tariff.Update";
    public const string TariffApprove = "Tariff.Approve";

    public const string InvoiceRead = "Invoice.Read";
    public const string InvoiceWrite = "Invoice.Write";

    public static IReadOnlyList<string> All { get; } =
    [
        ShipmentRead, ShipmentCreate, ShipmentUpdate, ShipmentDelete,
        TariffRead, TariffUpdate, TariffApprove,
        InvoiceRead, InvoiceWrite
    ];
}
