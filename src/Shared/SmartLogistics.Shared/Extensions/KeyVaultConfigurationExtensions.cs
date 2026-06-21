using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace SmartLogistics.Shared.Extensions;

public static class KeyVaultConfigurationExtensions
{
    public static IConfigurationBuilder AddSmartLogisticsKeyVault(
        this IConfigurationBuilder configuration,
        IHostEnvironment environment)
    {
        var built = configuration.Build();
        var vaultUri = built["KeyVault:VaultUri"];

        if (string.IsNullOrWhiteSpace(vaultUri))
        {
            return configuration;
        }

        configuration.AddAzureKeyVault(new Uri(vaultUri), new Azure.Identity.DefaultAzureCredential());
        return configuration;
    }
}
