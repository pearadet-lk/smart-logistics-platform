using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace SmartLogistics.Ai.Api.Services;

public sealed class LogisticsAssistantService(
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration,
    ILogger<LogisticsAssistantService> logger)
{
    public async Task<string> AskAsync(string question, string? tenant, CancellationToken ct = default)
    {
        var endpoint = configuration["AzureOpenAI:Endpoint"];
        var apiKey = configuration["AzureOpenAI:ApiKey"];
        var deployment = configuration["AzureOpenAI:Deployment"] ?? "gpt-4o-mini";

        if (string.IsNullOrWhiteSpace(endpoint) || string.IsNullOrWhiteSpace(apiKey))
        {
            return MockResponse(question, tenant);
        }

        try
        {
            var client = httpClientFactory.CreateClient("azure-openai");
            client.DefaultRequestHeaders.Add("api-key", apiKey);

            var url = $"{endpoint.TrimEnd('/')}/openai/deployments/{deployment}/chat/completions?api-version=2024-06-01";
            var body = new
            {
                messages = new object[]
                {
                    new { role = "system", content = "You are a logistics assistant for Smart Logistics Platform. Answer concisely about shipments, tariffs, containers, and invoices." },
                    new { role = "user", content = $"Tenant: {tenant ?? "ALL"}. Question: {question}" }
                },
                max_tokens = 500
            };

            var response = await client.PostAsync(url,
                new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json"), ct);

            response.EnsureSuccessStatusCode();
            using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
            return doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString()
                   ?? MockResponse(question, tenant);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Azure OpenAI call failed — using mock assistant");
            return MockResponse(question, tenant);
        }
    }

    private static string MockResponse(string question, string? tenant) =>
        $"""
         [Mock AI Assistant — configure AzureOpenAI:Endpoint and ApiKey for live responses]
         Tenant: {tenant ?? "ALL"}
         Question: {question}

         Summary: Based on platform data, 42 shipments are in transit. 3 containers arrive at LAX next week.
         Delayed shipments for tenant: 2. Top route: BKK → LAX.
         """;
}
