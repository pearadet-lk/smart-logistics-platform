using System.Text.Json.Serialization;

namespace SmartLogistics.Shared.Auth;

internal sealed class KeycloakTokenResponse
{
    [JsonPropertyName("access_token")]
    public string? AccessToken { get; set; }

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }
}
