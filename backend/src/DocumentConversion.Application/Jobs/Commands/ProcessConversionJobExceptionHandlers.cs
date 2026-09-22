using DocumentConversion.Application.Jobs.Commands;
using DocumentConversion.Domain.Jobs.Enums;
using DocumentConversion.Domain.Jobs.Exceptions;
using DocumentConversion.Domain.Jobs.Repositories;
using DocumentConversion.Domain.Jobs.ValueObjects;
using MediatR;
using MediatR.Pipeline;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DocumentConversion.Application.Jobs.Commands;

internal static class ProcessConversionJobFailureRecorder
{
    public static Task FailAsync(
        IConversionJobRepository repository,
        Guid jobId,
        DomainErrorCode code,
        string message,
        CancellationToken cancellationToken) =>
        repository.FailJobAsync(JobId.From(jobId), code, message, cancellationToken);
}

public sealed class ProcessConversionJobDomainExceptionHandler
    : IRequestExceptionHandler<ProcessConversionJobCommand, Unit, DomainException>
{
    private readonly IServiceScopeFactory _scopeFactory;

    public ProcessConversionJobDomainExceptionHandler(IServiceScopeFactory scopeFactory) =>
        _scopeFactory = scopeFactory;

    public async Task Handle(
        ProcessConversionJobCommand request,
        DomainException exception,
        RequestExceptionHandlerState<Unit> state,
        CancellationToken cancellationToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var repository = scope.ServiceProvider.GetRequiredService<IConversionJobRepository>();
        await ProcessConversionJobFailureRecorder.FailAsync(
            repository,
            request.JobId,
            exception.DomainCode,
            exception.Message,
            cancellationToken);

        state.SetHandled(Unit.Value);
    }
}

public sealed class ProcessConversionJobUnexpectedExceptionHandler
    : IRequestExceptionHandler<ProcessConversionJobCommand, Unit, Exception>
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ProcessConversionJobUnexpectedExceptionHandler> _logger;

    public ProcessConversionJobUnexpectedExceptionHandler(
        IServiceScopeFactory scopeFactory,
        ILogger<ProcessConversionJobUnexpectedExceptionHandler> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task Handle(
        ProcessConversionJobCommand request,
        Exception exception,
        RequestExceptionHandlerState<Unit> state,
        CancellationToken cancellationToken)
    {
        if (exception is DomainException)
            return;

        _logger.LogError(exception, "Unexpected error processing conversion job {JobId}", request.JobId);

        await using var scope = _scopeFactory.CreateAsyncScope();
        var repository = scope.ServiceProvider.GetRequiredService<IConversionJobRepository>();
        await ProcessConversionJobFailureRecorder.FailAsync(
            repository,
            request.JobId,
            DomainErrorCode.UnexpectedError,
            "An unexpected error occurred during processing.",
            cancellationToken);

        state.SetHandled(Unit.Value);
    }
}
