using DocumentConversion.Application.Abstractions;
using DocumentConversion.Application.Jobs.Commands;
using DocumentConversion.Domain.Jobs.ValueObjects;
using Hangfire;
using Microsoft.Extensions.DependencyInjection;

namespace DocumentConversion.Infrastructure.Hangfire;

internal sealed class HangfireConversionJobScheduler(IBackgroundJobClient client) : IConversionJobScheduler
{
    private readonly IBackgroundJobClient _client = client;

    public void EnqueueProcessJob(JobId jobId) =>
        _client.Enqueue<ProcessConversionJobExecutor>(x => x.Run(jobId.Value));
}

public sealed class ProcessConversionJobExecutor(IServiceScopeFactory scopeFactory)
{
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;

    [AutomaticRetry(Attempts = 0)]
    public async Task Run(Guid jobId)
    {
        using var scope = _scopeFactory.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<MediatR.IMediator>();
        await mediator.Send(new ProcessConversionJobCommand(jobId));
    }
}
