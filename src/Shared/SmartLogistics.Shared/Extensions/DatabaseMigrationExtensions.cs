using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SmartLogistics.Shared.Extensions;

public static class DatabaseMigrationExtensions
{
    public static async Task MigrateSmartLogisticsDatabaseAsync<TContext>(
        this IHost host,
        Func<TContext, Task>? seed = null) where TContext : DbContext
    {
        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<TContext>>();

        logger.LogInformation("Applying EF migrations for {Context}", typeof(TContext).Name);
        await db.Database.MigrateAsync();

        if (seed is not null)
        {
            await seed(db);
        }
    }
}
