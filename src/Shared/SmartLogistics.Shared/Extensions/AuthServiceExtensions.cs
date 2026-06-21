using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using SmartLogistics.Shared.Authorization;

namespace SmartLogistics.Shared.Extensions;

public static class AuthServiceExtensions
{
    public static IServiceCollection AddSmartLogisticsAuth(this IServiceCollection services, IConfiguration configuration)
    {
        var authority = configuration["Keycloak:Authority"]
            ?? throw new InvalidOperationException("Keycloak:Authority is required.");

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = authority;
                options.Audience = configuration["Keycloak:Audience"];
                options.RequireHttpsMetadata = configuration.GetValue("Keycloak:RequireHttpsMetadata", false);
                options.TokenValidationParameters = new()
                {
                    ValidateAudience = !string.IsNullOrEmpty(configuration["Keycloak:Audience"]),
                    RoleClaimType = "roles"
                };
            });

        services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();

        services.AddAuthorization(options =>
        {
            options.AddPolicy("Authenticated", policy => policy.RequireAuthenticatedUser());

            foreach (var permission in Permissions.All)
            {
                options.AddPolicy(permission, policy =>
                    policy.Requirements.Add(new PermissionRequirement(permission)));
            }

            options.AddPolicy("RequireAdmin", policy => policy.RequireRole("Admin"));
            options.AddPolicy("RequireFinance", policy => policy.RequireRole("Finance", "Admin"));
        });

        return services;
    }
}
