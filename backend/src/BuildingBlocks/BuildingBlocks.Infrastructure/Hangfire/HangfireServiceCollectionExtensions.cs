using Hangfire;
using Hangfire.Storage.SQLite;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Infrastructure.Hangfire;

public static class HangfireServiceCollectionExtensions
{
    public static IServiceCollection AddBuildingBlocksHangfireSqlite(
        this IServiceCollection services,
        string hangfireConnectionString,
        bool addServer = true)
    {
        services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseSQLiteStorage(hangfireConnectionString));

        if (addServer)
            services.AddHangfireServer();

        return services;
    }
}
