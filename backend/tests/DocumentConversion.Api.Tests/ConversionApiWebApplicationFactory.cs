using DocumentConversion.Application.Abstractions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace DocumentConversion.Api.Tests;

public sealed class ConversionApiWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _contentRoot;
    private readonly string _appDbPath;
    private readonly string _hangfireDbPath;
    private SynchronousConversionJobScheduler? _scheduler;

    public SynchronousConversionJobScheduler Scheduler =>
        _scheduler ?? throw new InvalidOperationException("Test host not started yet.");

    public ConversionApiWebApplicationFactory()
    {
        _contentRoot = Path.Combine(Path.GetTempPath(), "doc-conv-api-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_contentRoot);
        _appDbPath = Path.Combine(_contentRoot, "app.db");
        _hangfireDbPath = Path.Combine(_contentRoot, "hangfire.db");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseContentRoot(_contentRoot);
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] = $"Data Source={_appDbPath}",
                ["ConnectionStrings:Hangfire"] = $"Data Source={_hangfireDbPath}"
            });
        });

        builder.ConfigureTestServices(services =>
        {
            var hangfireHosted = services
                .Where(d =>
                    d.ServiceType == typeof(IHostedService) &&
                    d.ImplementationType?.FullName?.Contains("Hangfire", StringComparison.Ordinal) == true)
                .ToList();
            foreach (var descriptor in hangfireHosted)
            {
                services.Remove(descriptor);
            }

            services.RemoveAll<IConversionJobScheduler>();
            services.AddSingleton<SynchronousConversionJobScheduler>();
            services.AddSingleton<IConversionJobScheduler>(sp => sp.GetRequiredService<SynchronousConversionJobScheduler>());
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);
        _scheduler = host.Services.GetRequiredService<SynchronousConversionJobScheduler>();
        return host;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing && Directory.Exists(_contentRoot))
        {
            try
            {
                Directory.Delete(_contentRoot, recursive: true);
            }
            catch (IOException)
            {
                // Best-effort cleanup on Windows file locks.
            }
        }
    }
}
