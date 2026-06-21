using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace SmartLogistics.Shared.Middleware;

/// <summary>Resolves tenant from JWT claim for multi-tenant data filtering.</summary>
public sealed class TenantMiddleware(RequestDelegate next)
{
    public const string TenantItemKey = "TenantId";

    public async Task InvokeAsync(HttpContext context)
    {
        var tenant = context.User.FindFirst("tenant")?.Value
            ?? context.User.FindFirst("company")?.Value;

        if (!string.IsNullOrWhiteSpace(tenant))
        {
            context.Items[TenantItemKey] = tenant;
        }

        await next(context);
    }
}

public static class TenantMiddlewareExtensions
{
    public static IApplicationBuilder UseTenantResolution(this IApplicationBuilder app) =>
        app.UseMiddleware<TenantMiddleware>();
}
