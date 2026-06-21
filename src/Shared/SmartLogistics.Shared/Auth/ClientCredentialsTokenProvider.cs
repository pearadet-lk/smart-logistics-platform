using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace SmartLogistics.Shared.Auth;

/// <summary>Keycloak Client Credentials flow for service-to-service calls.</summary>
public sealed class ClientCredentialsTokenProvider(
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration,
    IMemoryCache cache,
    ILogger<ClientCredentialsTokenProvider> logger)
{
    private const string CacheKey = "s2s-access-token";

    public async Task<string?> GetTokenAsync(CancellationToken cancellationToken = default)
    {
        if (cache.TryGetValue(CacheKey, out string? cached) && !string.IsNullOrEmpty(cached))
        {
            return cached;
        }

        var authority = configuration["Keycloak:Authority"];
        var clientId = configuration["Keycloak:ClientId"];
        var clientSecret = configuration["Keycloak:ClientSecret"];

        if (string.IsNullOrWhiteSpace(authority) || string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret))
        {
            logger.LogDebug("S2S credentials not configured — skipping token request");
            return null;
        }

        var client = httpClientFactory.CreateClient("keycloak-token");
        var tokenUrl = $"{authority.TrimEnd('/')}/protocol/openid-connect/token";

        var response = await client.PostAsync(tokenUrl, new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials",
            ["client_id"] = clientId,
            ["client_secret"] = clientSecret
        }), cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning("Client credentials token request failed: {Status}", response.StatusCode);
            return null;
        }

        var payload = await response.Content.ReadFromJsonAsync<KeycloakTokenResponse>(cancellationToken);
        if (payload?.AccessToken is null) return null;

        cache.Set(CacheKey, payload.AccessToken, TimeSpan.FromSeconds(Math.Max(60, payload.ExpiresIn - 60)));
        return payload.AccessToken;
    }
}

public static class ServiceAuthExtensions
{
    public static IServiceCollection AddSmartLogisticsServiceAuth(this IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddHttpClient("keycloak-token");
        services.AddSingleton<ClientCredentialsTokenProvider>();
        return services;
    }
}
