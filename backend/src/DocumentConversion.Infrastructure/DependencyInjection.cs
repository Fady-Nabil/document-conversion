using BuildingBlocks.Infrastructure.Hangfire;
using BuildingBlocks.Infrastructure.Persistence;
using DocumentConversion.Application.Abstractions;
using DocumentConversion.Application.Options;
using DocumentConversion.Domain.Jobs.Repositories;
using DocumentConversion.Infrastructure.Conversion;
using DocumentConversion.Infrastructure.Hangfire;
using DocumentConversion.Infrastructure.Persistence;
using DocumentConversion.Infrastructure.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DocumentConversion.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ConversionOptions>(configuration.GetSection(ConversionOptions.SectionName));

        var connectionString = configuration.GetConnectionString("Default") ?? "Data Source=app.db";
        services.AddBuildingBlocksSqliteDbContext<AppDbContext>(connectionString);

        services.AddScoped<IConversionJobRepository, ConversionJobRepository>();
        services.AddSingleton<IFileStorage, LocalFileStorage>();
        services.AddScoped<IPdfAnalyzer, PdfPigAnalyzer>();
        services.AddScoped<IDocumentConverter, OpenXmlDocumentConverter>();
        services.AddScoped<IConversionJobScheduler, HangfireConversionJobScheduler>();
        services.AddScoped<ProcessConversionJobExecutor>();

        var hangfireDb = configuration.GetConnectionString("Hangfire") ?? "Data Source=hangfire.db";
        services.AddBuildingBlocksHangfireSqlite(hangfireDb);

        return services;
    }
}
