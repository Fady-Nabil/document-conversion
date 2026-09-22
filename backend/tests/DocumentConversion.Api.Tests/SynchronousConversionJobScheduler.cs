using System.Collections.Concurrent;
using DocumentConversion.Application.Abstractions;
using DocumentConversion.Application.Jobs.Commands;
using DocumentConversion.Domain.Jobs.ValueObjects;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace DocumentConversion.Api.Tests;

public sealed class SynchronousConversionJobScheduler : IConversionJobScheduler
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ConcurrentQueue<Guid> _pending = new();

    public SynchronousConversionJobScheduler(IServiceScopeFactory scopeFactory) =>
        _scopeFactory = scopeFactory;

    public void EnqueueProcessJob(JobId jobId) => _pending.Enqueue(jobId.Value);

    public async Task DrainPendingAsync()
    {
        while (_pending.TryDequeue(out var jobId))
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            await mediator.Send(new ProcessConversionJobCommand(jobId));
        }
    }
}
