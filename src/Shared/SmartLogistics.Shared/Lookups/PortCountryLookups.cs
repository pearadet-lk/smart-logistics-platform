namespace SmartLogistics.Shared.Lookups;

public static class PortCountryLookups
{
    public static readonly IReadOnlyList<PortInfo> Ports =
    [
        new("BKK", "Bangkok", "TH", "Thailand"),
        new("LAX", "Los Angeles", "US", "United States"),
        new("LCB", "Laem Chabang", "TH", "Thailand"),
        new("SGN", "Ho Chi Minh City", "VN", "Vietnam"),
        new("SHA", "Shanghai", "CN", "China"),
        new("RTM", "Rotterdam", "NL", "Netherlands")
    ];

    public static readonly IReadOnlyList<CountryInfo> Countries =
    [
        new("TH", "Thailand"),
        new("US", "United States"),
        new("VN", "Vietnam"),
        new("CN", "China"),
        new("NL", "Netherlands"),
        new("SG", "Singapore")
    ];
}

public sealed record PortInfo(string Code, string Name, string CountryCode, string CountryName);
public sealed record CountryInfo(string Code, string Name);
